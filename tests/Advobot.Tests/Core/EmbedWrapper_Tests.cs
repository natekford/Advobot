using System;
using System.Collections.Generic;

using Advobot.Classes;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core
{
	[TestClass]
	public sealed class EmbedWrapper_Tests
	{
		private const string INVALID_URL = "not a url lol";
		private const string VALID_STRING = "Valid length string";
		private const string VALID_STRING_2 = "Second valid length string";
		private const string VALID_URL = "https://www.google.com";
		private readonly string LONG_ASS_STRING = new('A', 50000);
		private readonly string STRING_WITH_MANY_LINES = new('\n', 50);

		[TestMethod]
		public void Author_Test()
		{
			static void RunAuthorTest(Action<EmbedWrapper> action)
			{
				action(new EmbedWrapper
				{
					Author = new EmbedAuthorBuilder
					{
						Name = VALID_STRING,
					},
				});
			}

			RunAuthorTest(x =>
			{
				var success = x.TryAddAuthor(LONG_ASS_STRING, null, null, out var errors);
				Assert.IsFalse(success);
				Assert.AreEqual(2, errors.Count);
				Assert.AreEqual(VALID_STRING, x.Author.Name);
				Assert.IsNull(x.Author.IconUrl);
				Assert.IsNull(x.Author.Url);
			});
			RunAuthorTest(x =>
			{
				var success = x.TryAddAuthor(LONG_ASS_STRING, VALID_URL, VALID_URL, out var errors);
				Assert.IsFalse(success);
				Assert.AreEqual(2, errors.Count);
				Assert.AreEqual(VALID_STRING, x.Author.Name);
				Assert.IsNull(x.Author.IconUrl);
				Assert.IsNull(x.Author.Url);
			});
			RunAuthorTest(x =>
			{
				var success = x.TryAddAuthor(VALID_STRING_2, INVALID_URL, null, out var errors);
				Assert.IsFalse(success);
				Assert.AreEqual(1, errors.Count);
				Assert.AreEqual(VALID_STRING, x.Author.Name);
				Assert.IsNull(x.Author.IconUrl);
				Assert.IsNull(x.Author.Url);
			});
			RunAuthorTest(x =>
			{
				var success = x.TryAddAuthor(VALID_STRING_2, VALID_URL, null, out var errors);
				Assert.IsTrue(success);
				Assert.AreEqual(0, errors.Count);
				Assert.AreEqual(VALID_STRING_2, x.Author.Name);
				Assert.AreEqual(VALID_URL, x.Author.Url);
				Assert.IsNull(x.Author.IconUrl);
			});
		}

		[TestMethod]
		public void Description_Test()
		{
			static void RunDescriptionTest(Action<EmbedWrapper> action)
			{
				action(new EmbedWrapper
				{
					Description = VALID_STRING,
				});
			}

			RunDescriptionTest(x =>
			{
				var success = x.TryAddDescription(LONG_ASS_STRING, out var errors);
				Assert.IsFalse(success);
				Assert.AreEqual(2, errors.Count);
				Assert.AreEqual(VALID_STRING, x.Description);
			});
			RunDescriptionTest(x =>
			{
				var success = x.TryAddDescription(STRING_WITH_MANY_LINES, out var errors);
				Assert.IsFalse(success);
				Assert.AreEqual(1, errors.Count);
				Assert.AreEqual(VALID_STRING, x.Description);
			});
			RunDescriptionTest(x =>
			{
				var success = x.TryAddDescription(LONG_ASS_STRING + STRING_WITH_MANY_LINES, out var errors);
				Assert.IsFalse(success);
				Assert.AreEqual(3, errors.Count);
				Assert.AreEqual(VALID_STRING, x.Description);
			});
			RunDescriptionTest(x =>
			{
				var success = x.TryAddDescription(VALID_STRING_2, out var errors);
				Assert.IsTrue(success);
				Assert.AreEqual(0, errors.Count);
				Assert.AreEqual(VALID_STRING_2, x.Description);
			});
			RunDescriptionTest(x =>
			{
				FillWithRandomCrap(x);
				var success = x.TryAddDescription(VALID_STRING, out var errors);
				Assert.IsFalse(success);
				Assert.AreEqual(1, errors.Count);
			});
		}

		[TestMethod]
		public void ImageUrl_Test()
			=> UrlTest((e, v) => (e.TryAddImageUrl(v, out var r), r), x => x.ImageUrl);

		[TestMethod]
		public void ThumbnailUrl_Test()
			=> UrlTest((e, v) => (e.TryAddThumbnailUrl(v, out var r), r), x => x.ThumbnailUrl);

		[TestMethod]
		public void Title_Test()
		{
			static void RunTitleTest(Action<EmbedWrapper> action)
			{
				action(new EmbedWrapper
				{
					Title = VALID_STRING,
				});
			}

			RunTitleTest(x =>
			{
				var success = x.TryAddTitle(LONG_ASS_STRING, out var errors);
				Assert.IsFalse(success);
				Assert.AreEqual(2, errors.Count);
				Assert.AreEqual(VALID_STRING, x.Title);
			});

			RunTitleTest(x =>
			{
				var success = x.TryAddTitle(VALID_STRING_2, out var errors);
				Assert.IsTrue(success);
				Assert.AreEqual(0, errors.Count);
				Assert.AreEqual(VALID_STRING_2, x.Title);
			});
		}

		[TestMethod]
		public void Url_Test()
			=> UrlTest((e, v) => (e.TryAddUrl(v, out var r), r), x => x.Url);

		private void FillWithRandomCrap(EmbedWrapper wrapper)
		{
			wrapper.Title = new string('T', EmbedBuilder.MaxTitleLength);
			wrapper.Description = new string('D', EmbedBuilder.MaxDescriptionLength);
			wrapper.Author = new EmbedAuthorBuilder
			{
				Name = new string('A', EmbedAuthorBuilder.MaxAuthorNameLength),
			};
			wrapper.Footer = new EmbedFooterBuilder
			{
				Text = new string('F', EmbedFooterBuilder.MaxFooterTextLength),
			};
			wrapper.Fields.Clear();
			for (var i = 0; i < 10; ++i)
			{
				wrapper.Fields.Add(new EmbedFieldBuilder
				{
					Name = new string('M', EmbedFieldBuilder.MaxFieldNameLength),
					Value = new string('N', EmbedFieldBuilder.MaxFieldValueLength),
				});
			}
		}

		private void UrlTest(
					Func<EmbedWrapper, string, (bool, IReadOnlyList<IEmbedError>)> tryAdd,
			Func<EmbedWrapper, string?> getter)
		{
			static void RunUrlTest(Action<EmbedWrapper> action)
				=> action(new EmbedWrapper());

			RunUrlTest(x =>
			{
				var (success, errors) = tryAdd(x, VALID_URL);
				Assert.IsTrue(success);
				Assert.AreEqual(0, errors.Count);
				Assert.AreEqual(VALID_URL, getter(x));
			});
			RunUrlTest(x =>
			{
				var (success, errors) = tryAdd(x, INVALID_URL);
				Assert.IsFalse(success);
				Assert.AreEqual(1, errors.Count);
				Assert.AreEqual(null, getter(x));
			});
		}
	}
}