namespace SargentNexus.API.Tests;

public sealed class CanonicalContractAlignmentTests
{
	[Fact]
	public void CanonicalContract_IncludesExpectedRouteInventory()
	{
		var contract = ReadCanonicalContract();

		AssertRouteExists(contract, "PUT", "/api/v1/organizations/{organizationId}/logo");
		AssertRouteExists(contract, "GET", "/api/v1/users/{userId}");
		AssertRouteExists(contract, "POST", "/api/v1/users/{userId}/temporary-password");
		AssertRouteExists(contract, "GET", "/api/v1/organizations/{organizationId}/boards");
		AssertRouteExists(contract, "GET", "/api/v1/boards/{boardId}/ideas");
	}

	[Fact]
	public void CanonicalContract_TemporaryPasswordRoute_IsPostOnly()
	{
		var contract = ReadCanonicalContract();

		AssertRouteExists(contract, "POST", "/api/v1/users/{userId}/temporary-password");
		AssertRouteDoesNotExist(contract, "PUT", "/api/v1/users/{userId}/temporary-password");
	}

	[Fact]
	public void CanonicalContract_RepresentativeRoutes_DefineSuccessResponseSemantics()
	{
		var contract = ReadCanonicalContract();

		AssertSectionContains(contract, "GET", "/api/v1/auth/me", "Success response `200`");
		AssertSectionContains(contract, "POST", "/api/v1/organizations", "Success response `201`");
		AssertSectionContains(contract, "POST", "/api/v1/organizations/{organizationId}/boards", "Success response `201`");
		AssertSectionContains(contract, "POST", "/api/v1/boards/{boardId}/ideas", "Success response `201`");
		AssertSectionContains(contract, "POST", "/api/v1/ideas/{ideaId}/comments", "Success response `201`");
		AssertSectionContains(contract, "POST", "/api/v1/ideas/{ideaId}/upvote/toggle", "Success response `200`");
	}

	[Fact]
	public void CanonicalContract_DefinesProblemDetailsAndRepresentativeErrorResponses()
	{
		var contract = ReadCanonicalContract();

		Assert.Contains("All non-2xx responses use a problem-details-style payload.", contract, StringComparison.Ordinal);
		Assert.Contains("Validation failures may include an additional `errors` object keyed by field name.", contract, StringComparison.Ordinal);
		AssertSectionContains(contract, "POST", "/api/v1/auth/login", "- `401` invalid credentials");
		AssertSectionContains(contract, "POST", "/api/v1/auth/login", "- `403` inactive account");
		AssertSectionContains(contract, "POST", "/api/v1/auth/login", "- `429` locked out");
		AssertSectionContains(contract, "POST", "/api/v1/organizations", "- `400` request body is malformed");
		AssertSectionContains(contract, "PUT", "/api/v1/organizations/{organizationId}", "- `404` organization does not exist");
	}

	[Fact]
	public void CanonicalContract_IdeaAndCommentWrites_MatchRequiredShape()
	{
		var contract = ReadCanonicalContract();
		var createIdea = ExtractRouteSection(contract, "POST", "/api/v1/boards/{boardId}/ideas");
		var updateIdea = ExtractRouteSection(contract, "PUT", "/api/v1/ideas/{ideaId}");
		var createComment = ExtractRouteSection(contract, "POST", "/api/v1/ideas/{ideaId}/comments");

		Assert.Contains("`priority` required string", createIdea, StringComparison.Ordinal);
		Assert.Contains("`dueDate` optional date string (`YYYY-MM-DD`)", createIdea, StringComparison.Ordinal);
		Assert.Contains("`assigneeUserIds` optional array of zero to five distinct GUID strings", createIdea, StringComparison.Ordinal);
		Assert.Contains("`assigneeUserIds` optional array of zero to five distinct GUID strings", updateIdea, StringComparison.Ordinal);
		Assert.Contains("`body` required string, max 2000 characters", createComment, StringComparison.Ordinal);
	}

	[Fact]
	public void CanonicalContract_NotificationEvent_RequiresMessage()
	{
		var contract = ReadCanonicalContract();
		var notificationPayload = ExtractHeadingSection(contract, "### Internal notification event payload");

		Assert.Contains("- `message` human-readable event summary string", notificationPayload, StringComparison.Ordinal);
	}

	[Fact]
	public void CanonicalContract_DoesNotExposeDeferredOAuthOrSamlEndpoints()
	{
		var routeHeadings = SplitLines(ReadCanonicalContract())
			.Where(line => line.StartsWith("### `", StringComparison.Ordinal));

		Assert.DoesNotContain(routeHeadings, heading => heading.Contains("/api/v1/auth/oauth", StringComparison.OrdinalIgnoreCase));
		Assert.DoesNotContain(routeHeadings, heading => heading.Contains("/api/v1/auth/oidc", StringComparison.OrdinalIgnoreCase));
		Assert.DoesNotContain(routeHeadings, heading => heading.Contains("/api/v1/auth/saml", StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public void CanonicalContract_DoesNotExposeDeferredAuditOrNotificationQueryEndpoints()
	{
		var routeHeadings = SplitLines(ReadCanonicalContract())
			.Where(line => line.StartsWith("### `", StringComparison.Ordinal));

		Assert.DoesNotContain(routeHeadings, heading => heading.Contains("/api/v1/audit", StringComparison.OrdinalIgnoreCase));
		Assert.DoesNotContain(routeHeadings, heading => heading.Contains("/api/v1/notifications", StringComparison.OrdinalIgnoreCase));
	}

	private static void AssertRouteExists(string contract, string method, string path)
	{
		Assert.Contains(RouteHeading(method, path), contract, StringComparison.Ordinal);
	}

	private static void AssertRouteDoesNotExist(string contract, string method, string path)
	{
		Assert.DoesNotContain(RouteHeading(method, path), contract, StringComparison.Ordinal);
	}

	private static void AssertSectionContains(string contract, string method, string path, string expected)
	{
		Assert.Contains(expected, ExtractRouteSection(contract, method, path), StringComparison.Ordinal);
	}

	private static string ExtractRouteSection(string contract, string method, string path)
	{
		return ExtractHeadingSection(contract, RouteHeading(method, path));
	}

	private static string ExtractHeadingSection(string markdown, string heading)
	{
		var lines = SplitLines(markdown);
		var startIndex = FindLineIndex(lines, heading);
		if (startIndex < 0)
		{
			throw new Xunit.Sdk.XunitException($"Expected to find heading '{heading}'.");
		}

		var endIndex = Array.FindIndex(lines, startIndex + 1, line => line.StartsWith("### ", StringComparison.Ordinal));
		endIndex = endIndex < 0 ? lines.Length : endIndex;
		return string.Join(Environment.NewLine, lines[(startIndex + 1)..endIndex]);
	}

	private static string RouteHeading(string method, string path) => $"### `{method} {path}`";

	private static int FindLineIndex(IReadOnlyList<string> lines, string exactLine)
	{
		for (var index = 0; index < lines.Count; index++)
		{
			if (string.Equals(lines[index].TrimEnd(), exactLine, StringComparison.Ordinal))
			{
				return index;
			}
		}

		return -1;
	}

	private static string[] SplitLines(string text)
	{
		return text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
	}

	private static string ReadCanonicalContract() => ReadRepoFile("SPEC", "30-Contracts.md");

	private static string ReadRepoFile(params string[] relativePathSegments)
	{
		var fullPath = Path.Combine(GetRepositoryRootDirectory(), Path.Combine(relativePathSegments));
		return File.ReadAllText(fullPath);
	}

	private static string GetRepositoryRootDirectory()
	{
		var current = new DirectoryInfo(AppContext.BaseDirectory);
		while (current is not null)
		{
			if (File.Exists(Path.Combine(current.FullName, "SargentNexus.sln")))
			{
				return current.FullName;
			}

			current = current.Parent;
		}

		throw new Xunit.Sdk.XunitException("Unable to locate repository root from test base directory.");
	}
}
