using Advobot.Embeds;

using Discord;

namespace Advobot.Tests.Core.Embeds;

[TestClass]
public sealed class EmbedWrapper_Tests
{
	private const string INITIAL = "Valid length string";
	private const string INVALID_URL = "not a url lol";
	private const string NEW = "Second valid length string";
	private const string VALID_URL = "https://www.google.com";
	private static readonly string LONG = new('A', 50000);

	[TestMethod]
	public void AuthorInvalidUrl_Test()
	{
		RunAuthorTest(embed =>
		{
			var success = embed.TrySetAuthor(NEW, INVALID_URL, null, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(1, errors!);
			Assert.AreEqual(INITIAL, embed.Author?.Name);
			Assert.IsNull(embed.Author?.IconUrl);
			Assert.IsNull(embed.Author?.Url);
		});
		RunAuthorTest(embed =>
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				embed.ThrowOnError = true;
				embed.Author = new()
				{
					Name = NEW,
					Url = INVALID_URL,
				};
			});
			Assert.AreEqual(INITIAL, embed.Author?.Name);
			Assert.IsNull(embed.Author?.IconUrl);
			Assert.IsNull(embed.Author?.Url);
		});
	}

	[TestMethod]
	public void AuthorNull_Test()
	{
		RunAuthorTest(embed =>
		{
			embed.Author = null;
			Assert.IsNull(embed.Author);
		});
	}

	[TestMethod]
	public void AuthorTooLong_Test()
	{
		RunAuthorTest(embed =>
		{
			var success = embed.TrySetAuthor(LONG, VALID_URL, VALID_URL, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(2, errors!);
			Assert.AreEqual(INITIAL, embed.Author?.Name);
			Assert.IsNull(embed.Author?.IconUrl);
			Assert.IsNull(embed.Author?.Url);
		});
		RunAuthorTest(embed =>
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				embed.Author = new()
				{
					Name = LONG,
				};
			});
			Assert.AreEqual(INITIAL, embed.Author?.Name);
			Assert.IsNull(embed.Author?.IconUrl);
			Assert.IsNull(embed.Author?.Url);
		});
	}

	[TestMethod]
	public void AuthorTooLongNoUrls_Test()
	{
		RunAuthorTest(embed =>
		{
			var success = embed.TrySetAuthor(LONG, null, null, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(2, errors!);
			Assert.AreEqual(INITIAL, embed.Author?.Name);
			Assert.IsNull(embed.Author?.IconUrl);
			Assert.IsNull(embed.Author?.Url);
		});
		RunAuthorTest(embed =>
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				embed.Author = new()
				{
					Name = LONG,
				};
			});
			Assert.AreEqual(INITIAL, embed.Author?.Name);
			Assert.IsNull(embed.Author?.IconUrl);
			Assert.IsNull(embed.Author?.Url);
		});
	}

	[TestMethod]
	public void AuthorValid_Test()
	{
		RunAuthorTest(embed =>
		{
			var success = embed.TrySetAuthor(NEW, VALID_URL, null, out var errors);
			Assert.IsTrue(success);
			Assert.IsNull(errors);
			Assert.AreEqual(NEW, embed.Author?.Name);
			Assert.AreEqual(VALID_URL, embed.Author?.Url);
			Assert.IsNull(embed.Author?.IconUrl);
		});
		RunAuthorTest(embed =>
		{
			embed.Author = new()
			{
				Name = NEW,
				Url = VALID_URL,
			};
			Assert.AreEqual(NEW, embed.Author?.Name);
			Assert.AreEqual(VALID_URL, embed.Author?.Url);
			Assert.IsNull(embed.Author?.IconUrl);
		});
	}

	[TestMethod]
	public void DescriptionFilled_Test()
	{
		RunFilledTest(embed =>
		{
			var success = embed.TrySetDescription(INITIAL, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(1, errors!);
			Assert.AreEqual(EmbedBuilder.MaxDescriptionLength, embed.Description?.Length);
		});
		RunFilledTest(embed =>
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				embed.ThrowOnError = true;
				embed.Description = NEW;
			});
			Assert.AreEqual(EmbedBuilder.MaxDescriptionLength, embed.Description?.Length);
		});
	}

	[TestMethod]
	public void DescriptionTooLong_Test()
	{
		RunDescriptionTest(embed =>
		{
			var success = embed.TrySetDescription(LONG, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(2, errors!);
			Assert.AreEqual(INITIAL, embed.Description);
		});
		RunDescriptionTest(embed =>
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				embed.ThrowOnError = true;
				embed.Description = LONG;
			});
			Assert.AreEqual(INITIAL, embed.Description);
		});
	}

	[TestMethod]
	public void DescriptionValid_Test()
	{
		RunDescriptionTest(embed =>
		{
			var success = embed.TrySetDescription(NEW, out var errors);
			Assert.IsTrue(success);
			Assert.IsNull(errors);
			Assert.AreEqual(NEW, embed.Description);
		});
		RunDescriptionTest(embed =>
		{
			embed.Description = NEW;
			Assert.AreEqual(NEW, embed.Description);
		});
	}

	[TestMethod]
	public void FieldEmpty_Test()
	{
		RunFieldTest(embed =>
		{
			var success = embed.TryAddField(null, null, true, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(2, errors!);
			Assert.HasCount(1, embed.Fields);
		});
		RunFieldTest(embed =>
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				embed.Fields.Add(new()
				{
					Name = null,
					Value = null,
				});
			});
			Assert.HasCount(1, embed.Fields);
		});
	}

	[TestMethod]
	public void FieldTooLong_Test()
	{
		RunFieldTest(embed =>
		{
			var success = embed.TryAddField(LONG, INITIAL, true, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(2, errors!);
			Assert.HasCount(1, embed.Fields);
		});
		RunFieldTest(embed =>
		{
			var success = embed.TryAddField(INITIAL, LONG, true, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(2, errors!);
			Assert.HasCount(1, embed.Fields);
		});
		RunFieldTest(embed =>
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				embed.Fields.Add(new()
				{
					Name = LONG,
					Value = LONG,
				});
			});
			Assert.HasCount(1, embed.Fields);
		});
	}

	[TestMethod]
	public void FieldTooMany_Test()
	{
		RunFieldTest(embed =>
		{
			embed.Fields = [.. Enumerable.Range(0, 25).Select(x => new EmbedFieldBuilder
			{
				Name = x.ToString(),
				Value = x.ToString(),
			})];
			var success = embed.TryAddField(INITIAL, INITIAL, true, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(1, errors!);
			Assert.HasCount(25, embed.Fields);
		});
	}

	[TestMethod]
	public void FieldValid_Test()
	{
		RunFieldTest(embed =>
		{
			var success = embed.TryAddField(INITIAL, INITIAL, true, out var errors);
			Assert.IsTrue(success);
			Assert.IsNull(errors);
			Assert.HasCount(2, embed.Fields);
		});
	}

	[TestMethod]
	public void FooterInvalidUrl_Test()
	{
		RunFooterTest(embed =>
		{
			var success = embed.TrySetFooter(LONG, INVALID_URL, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(3, errors!);
			Assert.AreEqual(INITIAL, embed.Footer?.Text);
			Assert.IsNull(embed.Footer?.IconUrl);
		});
		RunFooterTest(embed =>
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				embed.Footer = new()
				{
					Text = LONG,
					IconUrl = INVALID_URL,
				};
			});
			Assert.AreEqual(INITIAL, embed.Footer?.Text);
			Assert.IsNull(embed.Footer?.IconUrl);
		});
	}

	[TestMethod]
	public void FooterNull_Test()
	{
		RunFooterTest(embed =>
		{
			embed.Footer = null;
			Assert.IsNull(embed.Footer);
		});
	}

	[TestMethod]
	public void FooterTooLong_Test()
	{
		RunFooterTest(embed =>
		{
			var success = embed.TrySetFooter(LONG, VALID_URL, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(2, errors!);
			Assert.AreEqual(INITIAL, embed.Footer?.Text);
			Assert.IsNull(embed.Footer?.IconUrl);
		});
		RunFooterTest(embed =>
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				embed.Footer = new()
				{
					Text = LONG,
					IconUrl = VALID_URL,
				};
			});
			Assert.AreEqual(INITIAL, embed.Footer?.Text);
			Assert.IsNull(embed.Footer?.IconUrl);
		});
	}

	[TestMethod]
	public void FooterTooLongNoUrls_Test()
	{
		RunFooterTest(embed =>
		{
			var success = embed.TrySetFooter(LONG, null, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(2, errors!);
			Assert.AreEqual(INITIAL, embed.Footer?.Text);
			Assert.IsNull(embed.Footer?.IconUrl);
		});
		RunFooterTest(embed =>
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				embed.Footer = new()
				{
					Text = LONG,
				};
			});
			Assert.AreEqual(INITIAL, embed.Footer?.Text);
			Assert.IsNull(embed.Footer?.IconUrl);
		});
	}

	[TestMethod]
	public void FooterValid_Test()
	{
		RunFooterTest(embed =>
		{
			var success = embed.TrySetFooter(INITIAL, null, out var errors);
			Assert.IsTrue(success);
			Assert.IsNull(errors);
		});
	}

	[TestMethod]
	public void ImageUrl_Test()
		=> RunUrlTest((e, v) => (e.TrySetImageUrl(v, out var ex), ex), x => x.ImageUrl);

	[TestMethod]
	public void ThumbnailUrl_Test()
		=> RunUrlTest((e, v) => (e.TrySetThumbnailUrl(v, out var ex), ex), x => x.ThumbnailUrl);

	[TestMethod]
	public void TitleFilled_Test()
	{
		RunFilledTest(embed =>
		{
			var success = embed.TrySetTitle(NEW, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(1, errors!);
			Assert.AreEqual(EmbedBuilder.MaxTitleLength, embed.Title?.Length);
		});
		RunFilledTest(embed =>
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				embed.ThrowOnError = true;
				embed.Title = NEW;
			});
			Assert.AreEqual(EmbedBuilder.MaxTitleLength, embed.Title?.Length);
		});
	}

	[TestMethod]
	public void TitleTooLong_Test()
	{
		RunTitleTest(embed =>
		{
			var success = embed.TrySetTitle(LONG, out var errors);
			Assert.IsFalse(success);
			Assert.HasCount(2, errors!);
			Assert.AreEqual(INITIAL, embed.Title);
		});
		RunTitleTest(embed =>
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				embed.ThrowOnError = true;
				embed.Title = LONG;
			});
			Assert.AreEqual(INITIAL, embed.Title);
		});
	}

	[TestMethod]
	public void TitleValidLength_Test()
	{
		RunTitleTest(embed =>
		{
			var success = embed.TrySetTitle(NEW, out var errors);
			Assert.IsTrue(success);
			Assert.IsNull(errors);
			Assert.AreEqual(NEW, embed.Title);
		});
		RunTitleTest(embed =>
		{
			embed.Title = NEW;
			Assert.AreEqual(NEW, embed.Title);
		});
	}

	[TestMethod]
	public void Url_Test()
		=> RunUrlTest((e, v) => (e.TrySetUrl(v, out var ex), ex), x => x.Url);

	private static void RunAuthorTest(Action<EmbedWrapper> action)
	{
		action(new()
		{
			Author = new()
			{
				Name = INITIAL,
			},
		});
	}

	private static void RunDescriptionTest(Action<EmbedWrapper> action)
	{
		action(new()
		{
			Description = INITIAL,
		});
	}

	private static void RunFieldTest(Action<EmbedWrapper> action)
	{
		action(new()
		{
			Fields =
			[
				new()
				{
					Name = INITIAL,
					Value = INITIAL,
				}
			],
		});
	}

	private static void RunFilledTest(Action<EmbedWrapper> action)
	{
		action(new(new()
		{
			Title = new('A', EmbedBuilder.MaxTitleLength),
			Author = new()
			{
				Name = new('A', EmbedAuthorBuilder.MaxAuthorNameLength),
			},
			Description = new('A', EmbedBuilder.MaxDescriptionLength),
			Footer = new()
			{
				Text = new('A', EmbedFooterBuilder.MaxFooterTextLength),
			},
			Fields = [.. Enumerable.Range(0, 25).Select(_ =>
			{
				return new EmbedFieldBuilder()
				{
					Name = new('A', EmbedFieldBuilder.MaxFieldNameLength),
					Value = new string('A', EmbedFieldBuilder.MaxFieldValueLength),
				};
			})],
		}));
	}

	private static void RunFooterTest(Action<EmbedWrapper> action)
	{
		action(new()
		{
			Footer = new()
			{
				Text = INITIAL,
			},
		});
	}

	private static void RunTitleTest(Action<EmbedWrapper> action)
	{
		action(new()
		{
			Title = INITIAL,
		});
	}

	private static void RunUrlTest(
		Func<EmbedWrapper, string?, (bool, IReadOnlyList<Exception>?)> tryAdd,
		Func<EmbedWrapper, string?> getter)
	{
		static void RunUrlTest(Action<EmbedWrapper> action)
			=> action(new EmbedWrapper());

		RunUrlTest(embed =>
		{
			var (success, errors) = tryAdd(embed, VALID_URL);
			Assert.IsTrue(success);
			Assert.IsNull(errors);
			Assert.AreEqual(VALID_URL, getter(embed));
		});
		RunUrlTest(embed =>
		{
			var (success, errors) = tryAdd(embed, INVALID_URL);
			Assert.IsFalse(success);
			Assert.HasCount(1, errors!);
			Assert.IsNull(getter(embed));
		});
		RunUrlTest(embed =>
		{
			var (success, errors) = tryAdd(embed, null);
			Assert.IsTrue(success);
			Assert.IsNull(errors);
			Assert.IsNull(getter(embed));
		});
	}
}