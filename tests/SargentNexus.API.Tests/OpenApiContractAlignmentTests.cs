using System.Text;

namespace SargentNexus.API.Tests;

public sealed class OpenApiContractAlignmentTests
{
	[Fact]
	public void MergedOpenApi_IncludesExpectedRouteInventory()
	{
		var mergedOpenApi = ReadRepoFile("SPEC", "SPECKIT", "openapi", "openapi.yaml");

		Assert.Contains("/api/v1/organizations/{organizationId}/logo:", mergedOpenApi, StringComparison.Ordinal);
		Assert.Contains("/api/v1/users/{userId}:", mergedOpenApi, StringComparison.Ordinal);
		Assert.Contains("/api/v1/users/{userId}/temporary-password:", mergedOpenApi, StringComparison.Ordinal);
	}

	[Fact]
	public void OrganizationsUsersPaths_TemporaryPasswordBlock_DoesNotContainPutMethod()
	{
		var organizationsUsersPaths = ReadRepoFile("SPEC", "SPECKIT", "openapi", "paths", "organizations-users.yaml");
		var temporaryPasswordPathBlock = ExtractPathBlock(organizationsUsersPaths, "/api/v1/users/{userId}/temporary-password");

		Assert.Contains("    post:", temporaryPasswordPathBlock, StringComparison.Ordinal);
		Assert.DoesNotContain("    put:", temporaryPasswordPathBlock, StringComparison.Ordinal);
	}

	[Fact]
	public void MergedPaths_Expected2xxResponsesWithBody_IncludeApplicationJsonSchema()
	{
		var authPaths = ReadRepoFile("SPEC", "SPECKIT", "openapi", "paths", "auth.yaml");
		AssertResponseHasApplicationJsonSchema(authPaths, "/api/v1/auth/me", "get", "200");

		var organizationsUsersPaths = ReadRepoFile("SPEC", "SPECKIT", "openapi", "paths", "organizations-users.yaml");
		AssertResponseHasApplicationJsonSchema(organizationsUsersPaths, "/api/v1/organizations", "get", "200");
		AssertResponseHasApplicationJsonSchema(organizationsUsersPaths, "/api/v1/organizations", "post", "201");
		AssertResponseHasApplicationJsonSchema(organizationsUsersPaths, "/api/v1/organizations/{organizationId}", "get", "200");
		AssertResponseHasApplicationJsonSchema(organizationsUsersPaths, "/api/v1/organizations/{organizationId}", "put", "200");
		AssertResponseHasApplicationJsonSchema(organizationsUsersPaths, "/api/v1/organizations/{organizationId}/users", "get", "200");
		AssertResponseHasApplicationJsonSchema(organizationsUsersPaths, "/api/v1/organizations/{organizationId}/users", "post", "201");
		AssertResponseHasApplicationJsonSchema(organizationsUsersPaths, "/api/v1/users/{userId}", "get", "200");
		AssertResponseHasApplicationJsonSchema(organizationsUsersPaths, "/api/v1/users/{userId}", "put", "200");

		var boardsStatusesPaths = ReadRepoFile("SPEC", "SPECKIT", "openapi", "paths", "boards-statuses.yaml");
		AssertResponseHasApplicationJsonSchema(boardsStatusesPaths, "/api/v1/organizations/{organizationId}/statuses", "get", "200");
		AssertResponseHasApplicationJsonSchema(boardsStatusesPaths, "/api/v1/organizations/{organizationId}/statuses", "post", "201");
		AssertResponseHasApplicationJsonSchema(boardsStatusesPaths, "/api/v1/statuses/{statusId}", "put", "200");
		AssertResponseHasApplicationJsonSchema(boardsStatusesPaths, "/api/v1/organizations/{organizationId}/boards", "get", "200");
		AssertResponseHasApplicationJsonSchema(boardsStatusesPaths, "/api/v1/organizations/{organizationId}/boards", "post", "201");
		AssertResponseHasApplicationJsonSchema(boardsStatusesPaths, "/api/v1/boards/{boardId}", "get", "200");
		AssertResponseHasApplicationJsonSchema(boardsStatusesPaths, "/api/v1/boards/{boardId}", "put", "200");

		var ideasEngagementPaths = ReadRepoFile("SPEC", "SPECKIT", "openapi", "paths", "ideas-engagement.yaml");
		AssertResponseHasApplicationJsonSchema(ideasEngagementPaths, "/api/v1/organizations/{organizationId}/tags", "get", "200");
		AssertResponseHasApplicationJsonSchema(ideasEngagementPaths, "/api/v1/boards/{boardId}/ideas", "get", "200");
		AssertResponseHasApplicationJsonSchema(ideasEngagementPaths, "/api/v1/boards/{boardId}/ideas", "post", "201");
		AssertResponseHasApplicationJsonSchema(ideasEngagementPaths, "/api/v1/ideas/{ideaId}", "get", "200");
		AssertResponseHasApplicationJsonSchema(ideasEngagementPaths, "/api/v1/ideas/{ideaId}", "put", "200");
		AssertResponseHasApplicationJsonSchema(ideasEngagementPaths, "/api/v1/ideas/{ideaId}/comments", "get", "200");
		AssertResponseHasApplicationJsonSchema(ideasEngagementPaths, "/api/v1/ideas/{ideaId}/comments", "post", "201");
		AssertResponseHasApplicationJsonSchema(ideasEngagementPaths, "/api/v1/comments/{commentId}", "put", "200");
		AssertResponseHasApplicationJsonSchema(ideasEngagementPaths, "/api/v1/ideas/{ideaId}/upvote/toggle", "post", "200");
	}

	[Fact]
	public void Feature005_IdeaSchemas_MatchRequiredContractShape()
	{
		var featureContract = ReadRepoFile("SPEC", "SPECKIT", "specs", "005-ideas-and-engagement", "contracts", "openapi.yaml");

		var ideaWriteRequestSchema = ExtractSchemaBlock(featureContract, "IdeaWriteRequest");
		var requiredFields = ExtractRequiredFields(ideaWriteRequestSchema);
		Assert.Contains("priority", requiredFields);

		var dueDateProperty = ExtractPropertyBlock(ideaWriteRequestSchema, "dueDate");
		Assert.Contains("format: date", dueDateProperty, StringComparison.Ordinal);

		var assigneeUserIdProperty = ExtractPropertyBlock(ideaWriteRequestSchema, "assigneeUserId");
		Assert.Contains("format: uuid", assigneeUserIdProperty, StringComparison.Ordinal);
		Assert.Contains("nullable: true", assigneeUserIdProperty, StringComparison.Ordinal);

		var commentWriteRequestSchema = ExtractSchemaBlock(featureContract, "CommentWriteRequest");
		var bodyProperty = ExtractPropertyBlock(commentWriteRequestSchema, "body");
		Assert.Contains("maxLength: 2000", bodyProperty, StringComparison.Ordinal);
	}

	[Fact]
	public void Feature006_NotificationEvent_RequiresMessage()
	{
		var featureContract = ReadRepoFile("SPEC", "SPECKIT", "specs", "006-notifications-and-audit", "contracts", "openapi.yaml");
		var notificationEventSchema = ExtractSchemaBlock(featureContract, "NotificationEvent");

		var requiredFields = ExtractRequiredFields(notificationEventSchema);
		Assert.Contains("message", requiredFields);
	}

	[Fact]
	public void MergedOpenApi_DoesNotExposeDeferredOAuthOrSamlEndpoints()
	{
		var mergedOpenApi = ReadRepoFile("SPEC", "SPECKIT", "openapi", "openapi.yaml");

		Assert.DoesNotContain("/api/v1/auth/oauth", mergedOpenApi, StringComparison.OrdinalIgnoreCase);
		Assert.DoesNotContain("/api/v1/auth/oidc", mergedOpenApi, StringComparison.OrdinalIgnoreCase);
		Assert.DoesNotContain("/api/v1/auth/saml", mergedOpenApi, StringComparison.OrdinalIgnoreCase);
	}

	private static void AssertResponseHasApplicationJsonSchema(string yaml, string path, string method, string statusCode)
	{
		var pathBlock = ExtractPathBlock(yaml, path);
		var methodBlock = ExtractMethodBlock(pathBlock, method);
		var responseBlock = ExtractResponseBlock(methodBlock, statusCode);

		Assert.Contains("application/json:", responseBlock, StringComparison.Ordinal);
		Assert.Contains("schema:", responseBlock, StringComparison.Ordinal);
	}

	private static string ExtractPathBlock(string yaml, string path)
	{
		return ExtractBlock(yaml, $"  {path}:", line => line.StartsWith("  /api/", StringComparison.Ordinal));
	}

	private static string ExtractMethodBlock(string pathBlock, string method)
	{
		return ExtractBlock(pathBlock, $"    {method}:", line =>
			line.StartsWith("    ", StringComparison.Ordinal)
			&& !line.StartsWith("      ", StringComparison.Ordinal)
			&& line.TrimEnd().EndsWith(":", StringComparison.Ordinal));
	}

	private static string ExtractResponseBlock(string methodBlock, string statusCode)
	{
		return ExtractBlock(methodBlock, $"        '{statusCode}':", line =>
			line.StartsWith("        '", StringComparison.Ordinal)
			&& line.TrimEnd().EndsWith("':", StringComparison.Ordinal));
	}

	private static string ExtractSchemaBlock(string yaml, string schemaName)
	{
		return ExtractBlock(yaml, $"    {schemaName}:", line =>
			line.StartsWith("    ", StringComparison.Ordinal)
			&& !line.StartsWith("      ", StringComparison.Ordinal)
			&& line.TrimEnd().EndsWith(":", StringComparison.Ordinal));
	}

	private static string ExtractPropertyBlock(string schemaBlock, string propertyName)
	{
		return ExtractBlock(schemaBlock, $"        {propertyName}:", line =>
			line.StartsWith("        ", StringComparison.Ordinal)
			&& !line.StartsWith("          ", StringComparison.Ordinal)
			&& line.TrimEnd().EndsWith(":", StringComparison.Ordinal));
	}

	private static IReadOnlyCollection<string> ExtractRequiredFields(string schemaBlock)
	{
		var lines = SplitLines(schemaBlock);
		for (var index = 0; index < lines.Length; index++)
		{
			var line = lines[index].TrimEnd();
			if (!line.StartsWith("      required:", StringComparison.Ordinal))
			{
				continue;
			}

			var inlineStart = line.IndexOf('[', StringComparison.Ordinal);
			var inlineEnd = line.IndexOf(']', StringComparison.Ordinal);
			if (inlineStart >= 0 && inlineEnd > inlineStart)
			{
				return line[(inlineStart + 1)..inlineEnd]
					.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			}

			var fields = new List<string>();
			for (var listIndex = index + 1; listIndex < lines.Length; listIndex++)
			{
				var listLine = lines[listIndex];
				if (listLine.StartsWith("        - ", StringComparison.Ordinal))
				{
					fields.Add(listLine[10..].Trim());
					continue;
				}

				if (string.IsNullOrWhiteSpace(listLine))
				{
					continue;
				}

				break;
			}

			return fields;
		}

		throw new Xunit.Sdk.XunitException("Expected to find a required field list in schema block.");
	}

	private static string ExtractBlock(string text, string header, Func<string, bool> isSiblingHeader)
	{
		var lines = SplitLines(text);
		var startIndex = FindLineIndex(lines, header);
		if (startIndex < 0)
		{
			throw new Xunit.Sdk.XunitException($"Expected to find header '{header}'.");
		}

		var builder = new StringBuilder();
		for (var index = startIndex + 1; index < lines.Length; index++)
		{
			var line = lines[index];
			if (isSiblingHeader(line))
			{
				break;
			}

			builder.AppendLine(line);
		}

		return builder.ToString();
	}

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
