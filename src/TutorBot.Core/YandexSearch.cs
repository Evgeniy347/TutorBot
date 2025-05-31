using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using TutorBot.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TutorBot.Core
{
    internal class YandexSearchService : IALService
    {
        private readonly YandexSearchOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly YandexSearchClient _client;
        internal YandexSearchService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _options = serviceProvider.GetRequiredService<IOptions<YandexSearchOptions>>().Value;
            _client = new YandexSearchClient(_options.IAMToken, _options.FolderId);
        }

        public async Task<string> TransferQuestionAL(long chatID, string currentMessage, Guid sessionID)
        {
            var request = new SearchRequest
            {
                Query = new QueryParameters
                {
                    QueryText = currentMessage,
                    SearchType = SearchType.Russian,
                    Page = 0
                },
                ResponseFormat = ResponseFormat.Xml,
                Region = Regions.Russia,
                Localization = Localizations.Russian,
                MaxPassages = 3,
                FolderId = string.Empty,
                GroupSpec = new GroupSpecification(),
                SortSpec = new SortSpecification(),
                UserAgent = "TutorBot"
            };

            var response = await _client.SearchAsync(request);

            // Get and display decoded content
            var content = response.GetDecodedContent();

            return content ?? string.Empty;
        }
    }

    public class YandexSearchOptions
    {
        public required string IAMToken { get; init; }
        public required string FolderId { get; init; }
    }

    /// <summary>
    /// Client for interacting with Yandex Search API v2
    /// </summary>
    public class YandexSearchClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _iamToken;
        private readonly string _folderId;
        private const string ApiEndpoint = "https://searchapi.api.cloud.yandex.net:443";

        /// <summary>
        /// Initialize a new instance of YandexSearchClient
        /// </summary>
        /// <param name="iamToken">IAM token for authentication</param>
        /// <param name="folderId">Cloud folder ID</param>
        /// <param name="httpClient">Optional HttpClient instance</param>
        public YandexSearchClient(string iamToken, string folderId, HttpClient? httpClient = null)
        {
            _iamToken = iamToken ?? throw new ArgumentNullException(nameof(iamToken));
            _folderId = folderId ?? throw new ArgumentNullException(nameof(folderId));

            _httpClient = httpClient ?? new HttpClient();
            _httpClient.BaseAddress = new Uri(ApiEndpoint);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Perform a synchronous web search
        /// </summary>
        /// <param name="request">Search request parameters</param>
        /// <returns>Search response</returns>
        public async Task<SearchResponse> SearchAsync(SearchRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Set the folder ID if not already set
            request.FolderId ??= _folderId;

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/yandex.cloud.searchapi.v2.WebSearchService/Search")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _iamToken);

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent);

            return new SearchResponse
            {
                RawData = Check.NotNull(apiResponse).RawData,
                Format = request.ResponseFormat
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Search request parameters
    /// </summary>
    public class SearchRequest
    {
        [JsonPropertyName("query")]
        public required QueryParameters Query { get; set; }

        [JsonPropertyName("sort_spec")]
        public required SortSpecification SortSpec { get; set; }

        [JsonPropertyName("group_spec")]
        public required GroupSpecification GroupSpec { get; set; }

        [JsonPropertyName("max_passages")]
        public int? MaxPassages { get; set; }

        [JsonPropertyName("region")]
        public int? Region { get; set; }

        [JsonPropertyName("l10n")]
        public required string Localization { get; set; }

        [JsonPropertyName("folder_id")]
        public required string FolderId { get; set; }

        [JsonPropertyName("response_format")]
        public ResponseFormat ResponseFormat { get; set; } = ResponseFormat.Xml;

        [JsonPropertyName("user_agent")]
        public required string UserAgent { get; set; }
    }

    /// <summary>
    /// Query parameters
    /// </summary>
    public class QueryParameters
    {
        [JsonPropertyName("search_type")]
        public SearchType SearchType { get; set; } = SearchType.Russian;

        [JsonPropertyName("query_text")]
        public required string QueryText { get; set; }

        [JsonPropertyName("family_filter")]
        public FamilyFilterMode FamilyFilter { get; set; } = FamilyFilterMode.Moderate;

        [JsonPropertyName("page")]
        public int Page { get; set; } = 0;

        [JsonPropertyName("fix_typo_mode")]
        public FixTypoMode FixTypoMode { get; set; } = FixTypoMode.On;
    }

    /// <summary>
    /// Sort specification
    /// </summary>
    public class SortSpecification
    {
        [JsonPropertyName("sort_mode")]
        public SortMode SortMode { get; set; } = SortMode.ByRelevance;

        [JsonPropertyName("sort_order")]
        public SortOrder SortOrder { get; set; } = SortOrder.Descending;
    }

    /// <summary>
    /// Group specification
    /// </summary>
    public class GroupSpecification
    {
        [JsonPropertyName("group_mode")]
        public GroupMode GroupMode { get; set; } = GroupMode.Deep;

        [JsonPropertyName("groups_on_page")]
        public int GroupsOnPage { get; set; } = 10;

        [JsonPropertyName("docs_in_group")]
        public int DocsInGroup { get; set; } = 1;
    }

    /// <summary>
    /// API response wrapper
    /// </summary>
    internal class ApiResponse
    {
        [JsonPropertyName("rawdata")]
        public required string RawData { get; set; }
    }

    /// <summary>
    /// Processed search response
    /// </summary>
    public class SearchResponse
    {
        /// <summary>
        /// Raw response data in Base64 encoding
        /// </summary>
        public required string RawData { get; set; }

        /// <summary>
        /// Response format (XML or HTML)
        /// </summary>
        public ResponseFormat Format { get; set; }

        /// <summary>
        /// Decodes the raw data from Base64
        /// </summary>
        /// <returns>Decoded content</returns>
        public string GetDecodedContent()
        {
            if (string.IsNullOrEmpty(RawData))
                return string.Empty;

            var bytes = Convert.FromBase64String(RawData);
            return Encoding.UTF8.GetString(bytes);
        }
    }

    /// <summary>
    /// Search type
    /// </summary>
    public enum SearchType
    {
        [JsonPropertyName("SEARCH_TYPE_RU")]
        Russian,
        [JsonPropertyName("SEARCH_TYPE_TR")]
        Turkish,
        [JsonPropertyName("SEARCH_TYPE_CON")]
        International,
        [JsonPropertyName("SEARCH_TYPE_KK")]
        Kazakh,
        [JsonPropertyName("SEARCH_TYPE_BE")]
        Belarusian
    }

    /// <summary>
    /// Family filter mode
    /// </summary>
    public enum FamilyFilterMode
    {
        [JsonPropertyName("FAMILY_MODE_MODERATE")]
        Moderate,
        [JsonPropertyName("FAMILY_MODE_NONE")]
        None,
        [JsonPropertyName("FAMILY_MODE_STRICT")]
        Strict
    }

    /// <summary>
    /// Fix typo mode
    /// </summary>
    public enum FixTypoMode
    {
        [JsonPropertyName("FIX_TYPO_MODE_ON")]
        On,
        [JsonPropertyName("FIX_TYPO_MODE_OFF")]
        Off
    }

    /// <summary>
    /// Sort mode
    /// </summary>
    public enum SortMode
    {
        [JsonPropertyName("SORT_MODE_BY_RELEVANCE")]
        ByRelevance,
        [JsonPropertyName("SORT_MODE_BY_TIME")]
        ByTime
    }

    /// <summary>
    /// Sort order
    /// </summary>
    public enum SortOrder
    {
        [JsonPropertyName("SORT_ORDER_DESC")]
        Descending,
        [JsonPropertyName("SORT_ORDER_ASC")]
        Ascending
    }

    /// <summary>
    /// Group mode
    /// </summary>
    public enum GroupMode
    {
        [JsonPropertyName("GROUP_MODE_DEEP")]
        Deep,
        [JsonPropertyName("GROUP_MODE_FLAT")]
        Flat
    }

    /// <summary>
    /// Response format
    /// </summary>
    public enum ResponseFormat
    {
        [JsonPropertyName("FORMAT_XML")]
        Xml,
        [JsonPropertyName("FORMAT_HTML")]
        Html
    }

    /// <summary>
    /// Common localizations
    /// </summary>
    public static class Localizations
    {
        public const string Russian = "LOCALIZATION_RU";
        public const string Belarusian = "LOCALIZATION_BE";
        public const string Kazakh = "LOCALIZATION_KK";
        public const string Ukrainian = "LOCALIZATION_UK";
        public const string Turkish = "LOCALIZATION_TR";
    }

    /// <summary>
    /// Common regions
    /// </summary>
    public static class Regions
    {
        public const int Russia = 225;
        public const int Turkey = 223;
        public const int Kazakhstan = 97;
        public const int Belarus = 149;
        public const int Ukraine = 187;
    }
}