﻿using Advobot.Core.Classes;
using Advobot.Core.Interfaces;
using Discord.Commands;
using ImageMagick;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Core.Utilities
{
	public static class ImageUtils
	{
		public const long MaxDownloadLengthInBytes = 10000000;
		public static readonly string FfmpegLocation = FindFfmpeg();

		/// <summary>
		/// Uses the image stream for <paramref name="callback"/>. Returns null if successful, returns an error string otherwise.
		/// The stream will be set to a position of 0 before the callback is invoked.
		/// </summary>
		/// <param name="uri">The uri to download the file from.</param>
		/// <param name="context">The current context.</param>
		/// <param name="args">The arguments to use on the file.</param>
		/// <param name="callback">What do to with the resized file.</param>
		/// <returns></returns>
		public static async Task<ResizedImageResult> ResizeImageAsync(Uri uri, ICommandContext context, IImageResizerArgs args)
		{
			var req = (HttpWebRequest)WebRequest.Create(uri.ToString().Replace(".gifv", ".mp4"));
			req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
			req.Credentials = CredentialCache.DefaultCredentials;
			req.Timeout = 5000;
			req.ReadWriteTimeout = 5000;
			//Not sure why the proxy has to be set, but with it being null I would get a PlatformNotSupportedException
			req.Proxy = new WebProxy();
			//For Imgur to redirect to correct page
			req.AllowAutoRedirect = true;

			var message = await context.Channel.SendMessageAsync("Starting to download the file.").CAF();
			WebResponse response = null;
			MemoryStream stream = null;
			try
			{
				response = await req.GetResponseAsync().CAF();
				if (args.ResizeTries < 1 && response.ContentLength > args.MaxAllowedLengthInBytes) //Max size without resize tries
				{
					return new ResizedImageResult(stream, default, $"file is bigger than the max allowed size of {(double)args.MaxAllowedLengthInBytes / 1000 * 1000:0.0}MB");
				}
				if (response.ContentLength > MaxDownloadLengthInBytes) //Utter max size, even with resize tries
				{
					return new ResizedImageResult(stream, default, $"file is bigger than the max allowed size of {(double)MaxDownloadLengthInBytes / 1000 * 1000:0.0}MB");
				}
				if (!Enum.TryParse<MagickFormat>(response.ContentType.Split('/').Last(), true, out var format) || !args.ValidFormats.Contains(format))
				{
					return new ResizedImageResult(stream, default, $"invalid file format supplied");
				}
				switch (format)
				{
					case MagickFormat.Jpg:
					case MagickFormat.Jpeg:
					case MagickFormat.Png:
						if (context.Guild.Emotes.Where(x => !x.Animated).Count() >= 50)
						{
							return new ResizedImageResult(stream, format, "there are already 50 non animated emotes");
						}
						break;
					case MagickFormat.Mp4:
						if (String.IsNullOrWhiteSpace(FfmpegLocation))
						{
							return new ResizedImageResult(stream, format, "mp4 is an invalid file format if ffmpeg is not installed");
						}
						goto case MagickFormat.Gif;
					case MagickFormat.Gif:
						if (context.Guild.Emotes.Where(x => x.Animated).Count() >= 50)
						{
							return new ResizedImageResult(stream, format, "there are already 50 animated emotes");
						}
						break;
					default:
						return new ResizedImageResult(stream, format, "link must lead to a png, jpg, gif, or mp4");
				}

				//Copy the response stream to a new variable so it can be seeked on
				await response.GetResponseStream().CopyToAsync(stream = new MemoryStream()).CAF();
				if (format == MagickFormat.Mp4) //Convert mp4 to gif so it can be used in animated gifs
				{
					await message.ModifyAsync(x => x.Content = $"Converting mp4 to gif.");
					await ConvertMp4ToGif(stream, (EmoteResizerArgs)args).CAF();
					format = MagickFormat.Gif;
				}
				if (stream.Length < args.MaxAllowedLengthInBytes)
				{
					return new ResizedImageResult(stream, format, null);
				}

				//Getting to this point has already checked resize tries, so this image needs to be resized if it's too big
				for (int i = 0; i < args.ResizeTries && stream.Length > args.MaxAllowedLengthInBytes; ++i)
				{
					await message.ModifyAsync(x => x.Content = $"Attempting to resize {i + 1}/{args.ResizeTries}.");
					if (ResizeFile(stream, args, format, i == 0, out var width, out var height)) //Acceptable size
					{
						break;
					}
					else if (width < 35 || height < 35) //Too small, will look like shit
					{
						return new ResizedImageResult(stream, format, $"during resizing the file has been made too small. Manually resize instead");
					}
					else if (i == args.ResizeTries - 1) //Too many attempts
					{
						return new ResizedImageResult(stream, format, $"failed to shrink the file to the max allowed size of {(double)args.MaxAllowedLengthInBytes / 1000 * 1000:0.0}MB");
					}
				}
				if (stream.Length < 1) //Stream somehow got empty, will result in error if callback is attempted
				{
					return new ResizedImageResult(stream, format, $"file is empty after shrinking");
				}

				return new ResizedImageResult(stream, format, null);
			}
			catch (Exception e)
			{
				return new ResizedImageResult(stream, default, e.Message);
			}
			//Not using using blocks because they cause the code to become too indented.
			finally
			{
				//Get rid of the update message
				await MessageUtils.DeleteMessageAsync(message, ClientUtils.CreateRequestOptions("image stream used")).CAF();
				if (response != null)
				{
					response.Dispose();
				}
				//stream isn't disposed here cause it's returned
			}
		}
		private static async Task ConvertMp4ToGif(MemoryStream ms, EmoteResizerArgs args)
		{
			var info = new ProcessStartInfo
			{
#if DEBUG
				CreateNoWindow = false,
#else
				CreateNoWindow = true,
#endif
				UseShellExecute = false,
				LoadUserProfile = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				FileName = FfmpegLocation,
				Arguments = $@"-f mp4 -i \\.\pipe\in -ss {args.StartInSeconds} -t {args.LengthInSeconds} -vf fps=12,scale=256:256 -f gif pipe:1",
			};
			using (var process = new Process { StartInfo = info, })
			//Have to use this pipe and not StandardInput b/c StandardInput hangs
			using (var inPipe = new NamedPipeServerStream("in", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, (int)ms.Length, (int)ms.Length))
			{
				process.Start();

				//Make sure the pipe is connected
				await inPipe.WaitForConnectionAsync().CAF();
				//Make sure to start at the beginning of the data to not get a "moov atom not found" error
				ms.Seek(0, SeekOrigin.Begin);
				await ms.CopyToAsync(inPipe).CAF();
				//Flush and close, otherwise hangs
				inPipe.Flush();
				inPipe.Close();

				//Clear and overwrite
				ms.SetLength(0);
				await process.StandardOutput.BaseStream.CopyToAsync(ms).CAF();
			}
		}
		private static bool ResizeFile(MemoryStream ms, IImageResizerArgs args, MagickFormat format, bool firstIter, out int width, out int height)
		{
			//Make sure at start
			ms.Seek(0, SeekOrigin.Begin);
			var shrinkFactor = Math.Sqrt((double)ms.Length / args.MaxAllowedLengthInBytes) * 1.1;
			switch (format)
			{
				case MagickFormat.Gif:
					using (var gif = new MagickImageCollection(ms))
					{
						//Determine the new width and height to give these frames
						var geo = new MagickGeometry
						{
							IgnoreAspectRatio = true, //Ignore aspect ratio so all the frames keep the same dimensions
							Width = width = (int)Math.Min(128, gif[0].Width / shrinkFactor),
							Height = height = (int)Math.Min(128, gif[0].Height / shrinkFactor),
						};
						foreach (var frame in gif)
						{
							frame.ColorFuzz = args.ColorFuzzing;
							frame.Scale(geo);
						}

						//Clear the stream and overwrite it
						ms.SetLength(0);
						gif.Write(ms);
					}
					return ms.Length < args.MaxAllowedLengthInBytes;
				case MagickFormat.Jpg:
				case MagickFormat.Jpeg:
				case MagickFormat.Png:
					using (var image = new MagickImage(ms))
					{
						image.Scale(new MagickGeometry
						{
							IgnoreAspectRatio = true,
							Width = width = (int)Math.Min(128, image.Width / shrinkFactor),
							Height = height = (int)Math.Min(128, image.Height / shrinkFactor),
						});

						//Clear the stream and overwrite it
						ms.SetLength(0);
						image.Write(ms);
					}
					return ms.Length < args.MaxAllowedLengthInBytes;
				default:
					throw new InvalidOperationException("This method only works on gif, png, and jpg formats.");
			}
		}
		private static string FindFfmpeg()
		{
			var windows = Environment.OSVersion.Platform.ToString().CaseInsContains("win");
			var ffmpeg = windows ? "ffmpeg.exe" : "ffmpeg";

			//Start with every special folder
			var directories = Enum.GetValues(typeof(Environment.SpecialFolder)).Cast<Environment.SpecialFolder>().Select(e =>
			{
				var p = Path.Combine(Environment.GetFolderPath(e), "ffmpeg");
				return Directory.Exists(p) ? new DirectoryInfo(p) : null;
			}).Where(x => x != null).ToList();
			//Look through where the program is stored
			if (Assembly.GetExecutingAssembly().Location is string assembly)
			{
				directories.Add(new DirectoryInfo(Path.GetDirectoryName(assembly)));
			}
			//Check path variables
			foreach (var part in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(windows ? ';' : ':'))
			{
				if (!String.IsNullOrWhiteSpace(part))
				{
					directories.Add(new DirectoryInfo(part.Trim()));
				}
			}
			//Look through every directory and any subfolders they have called bin
			foreach (var dir in directories.Select(x => new[] { x, new DirectoryInfo(Path.Combine(x.FullName, "bin")) }).SelectMany(x => x))
			{
				if (!dir.Exists)
				{
					continue;
				}

				var files = dir.GetFiles(ffmpeg, SearchOption.TopDirectoryOnly);
				if (files.Any())
				{
					return files[0].FullName;
				}
			}
			return null;
		}

		public sealed class ResizedImageResult : IDisposable
		{
			public MemoryStream Stream { get; }
			public MagickFormat Format { get; }
			public string Error { get; }
			public bool IsSuccess { get; }

			internal ResizedImageResult(MemoryStream stream, MagickFormat format, string error)
			{
				stream?.Seek(0, SeekOrigin.Begin);
				Stream = stream;
				Format = format;
				Error = error;
				IsSuccess = error == null && stream != null;
			}

			public void Dispose()
			{
				Stream?.Dispose();
			}
		}
	}
}
