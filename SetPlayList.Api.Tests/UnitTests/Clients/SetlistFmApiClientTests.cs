using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using SetPlayList.Api.Clients;
using SetPlayList.Api.Configuration;
using SetPlayList.Core.DTOs.SetlistFm;
using System.Net;
using System.Text.Json;
using SetPlayList.Api.Tests.UnitTests.Utilities;


namespace SetPlayList.Api.Tests.UnitTests.Clients;

public class SetlistFmApiClientTests
{
    private readonly Mock<IOptions<SetlistFmApiSettings>> _settingsMock;
    private readonly Mock<ILogger<SetlistFmApiClient>> _loggerMock;
    private readonly MockHttpMessageHandler _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly SetlistFmApiClient _sut;

    public SetlistFmApiClientTests()
    {
        _settingsMock = new Mock<IOptions<SetlistFmApiSettings>>();
        _settingsMock.Setup(s => s.Value).Returns(new SetlistFmApiSettings
        {
            ClientSecret = "2137"
        });

        _loggerMock = new Mock<ILogger<SetlistFmApiClient>>();
        _httpMessageHandlerMock = new MockHttpMessageHandler();
        _httpClient = _httpMessageHandlerMock.ToHttpClient();
        _sut = new SetlistFmApiClient(_httpClient, _settingsMock.Object, _loggerMock.Object);
    }

    #region GetSetlistAsync Tests

    [Fact]
    public async Task GetSetlistAsync_ValidSetlistIdValidSecret_ReturnsSetlistAnd200()
    {
        // Arrange
        var setlistId = "63ab8613";
        var expectedSetlist = new Setlist(
            Artist: new Artist(
                MBId: "b2d122f9-eadb-4930-a196-8f221eeb0c66",
                Name: "Rammstein",
                SortName: "Rammstein",
                Disambiguation: "",
                Url: "https://www.setlist.fm/setlists/rammstein-3bd43cd0.html"
            ),
            Venue: new Venue(
                City: new City(
                    Id: "2921466",
                    Name: "Gelsenkirchen",
                    StateCode: "07",
                    State: "North Rhine-Westphalia",
                    Coords: new Coords(
                        Long: 7.05,
                        Lat: 51.5166667
                    ),
                    Country: new Country(
                        Code: "DE",
                        Name: "Germany"
                    )
                ),
                Url: "https://www.setlist.fm/venue/veltins-arena-gelsenkirchen-germany-4bd78fbe.html",
                Id: "4bd78fbe",
                Name: "Veltins-Arena"
            ),
            Tour: new Tour(
                Name: "Stadium Tour"
            ),
            Sets: new Sets(
                Set: new List<Set>
                {
                    // Main Set
                    new Set(
                        Name: null,
                        Encore: 0,
                        Song: new List<Song>
                        {
                            new Song("Music for the Royal Fireworks", null, new Artist("27870d47-bb98-42d1-bf2b-c7e972e6befc", "George Frideric Handel", "Handel, George Frideric", "German‐British baroque composer", "https://www.setlist.fm/setlists/george-frideric-handel-7bd4ee68.html"), null, true),
                            new Song("Ramm 4", null, null, null, false),
                            new Song("Links 2 3 4", null, null, null, false),
                            new Song("Keine Lust", null, null, null, false),
                            new Song("Sehnsucht", null, null, null, false),
                            new Song("Asche zu Asche", null, null, null, false),
                            new Song("Mein Herz brennt", null, null, null, false),
                            new Song("Puppe", null, null, null, false),
                            new Song("Wiener Blut", null, null, null, false),
                            new Song("Zeit", null, null, null, false),
                            new Song("Deutschland", null, null, "Remix by Richard Z. Kruspe", true),
                            new Song("Deutschland", null, null, null, false),
                            new Song("Radio", null, null, null, false),
                            new Song("Mein Teil", null, null, null, false),
                            new Song("Du hast", null, null, null, false),
                            new Song("Sonne", null, null, null, false)
                        }
                    ),
                    // Encore 1
                    new Set(
                        Name: null,
                        Encore: 1,
                        Song: new List<Song>
                        {
                            new Song("Engel", new Artist("1e9d02a2-468a-4a53-ba46-6dbcd87b9595", "ABÉLARD", "Abélard", "French piano duo", "https://www.setlist.fm/setlists/abelard-53f78335.html"), null, "Piano version; performed on B-Stage", false),
                            new Song("Ausländer", null, null, null, false),
                            new Song("Du riechst so gut", null, null, null, false),
                            new Song("Pussy", null, null, null, false),
                            new Song("Ich will", null, null, null, false)
                        }
                    ),
                    // Encore 2
                    new Set(
                        Name: null,
                        Encore: 2,
                        Song: new List<Song>
                        {
                            new Song("Rammstein", null, null, null, false),
                            new Song("Adieu", null, null, null, false),
                            new Song("Sonne", null, null, "Piano version", true),
                            new Song("Haifisch", null, null, "Haiswing Remix by Olsen Involtini", true),
                            new Song("Lügen", null, null, "Instrumental", true)
                        }
                    )
                }
            ),
            Info: "Final show of the Stadium Tour as a whole.",
            Url: "https://www.setlist.fm/setlist/rammstein/2024/veltins-arena-gelsenkirchen-germany-63ab8613.html",
            Id: "63ab8613",
            VersionId: "g43ae3717",
            EventDate: "31-07-2024",
            LastUpdated: "2024-08-04T17:37:55.869+0000"
        );
        var responseJson = """
            {
            "id" : "63ab8613",
            "versionId" : "g43ae3717",
            "eventDate" : "31-07-2024",
            "lastUpdated" : "2024-08-04T17:37:55.869+0000",
            "artist" : {
            "mbid" : "b2d122f9-eadb-4930-a196-8f221eeb0c66",
            "name" : "Rammstein",
            "sortName" : "Rammstein",
            "disambiguation" : "",
            "url" : "https://www.setlist.fm/setlists/rammstein-3bd43cd0.html"
            },
            "venue" : {
            "id" : "4bd78fbe",
            "name" : "Veltins-Arena",
            "city" : {
            "id" : "2921466",
            "name" : "Gelsenkirchen",
            "state" : "North Rhine-Westphalia",
            "stateCode" : "07",
            "coords" : {
            "lat" : 51.5166667,
            "long" : 7.05
            },
            "country" : {
            "code" : "DE",
            "name" : "Germany"
            }
            },
            "url" : "https://www.setlist.fm/venue/veltins-arena-gelsenkirchen-germany-4bd78fbe.html"
            },
            "tour" : {
            "name" : "Stadium Tour"
            },
            "sets" : {
            "set" : [ {
            "song" : [ {
            "name" : "Music for the Royal Fireworks",
            "tape" : true,
            "cover" : {
            "mbid" : "27870d47-bb98-42d1-bf2b-c7e972e6befc",
            "name" : "George Frideric Handel",
            "sortName" : "Handel, George Frideric",
            "disambiguation" : "German‐British baroque composer",
            "url" : "https://www.setlist.fm/setlists/george-frideric-handel-7bd4ee68.html"
            }
            }, {
            "name" : "Ramm 4"
            }, {
            "name" : "Links 2 3 4"
            }, {
            "name" : "Keine Lust"
            }, {
            "name" : "Sehnsucht"
            }, {
            "name" : "Asche zu Asche"
            }, {
            "name" : "Mein Herz brennt"
            }, {
            "name" : "Puppe"
            }, {
            "name" : "Wiener Blut"
            }, {
            "name" : "Zeit"
            }, {
            "name" : "Deutschland",
            "tape" : true,
            "info" : "Remix by Richard Z. Kruspe"
            }, {
            "name" : "Deutschland"
            }, {
            "name" : "Radio"
            }, {
            "name" : "Mein Teil"
            }, {
            "name" : "Du hast"
            }, {
            "name" : "Sonne"
            } ]
            }, {
            "encore" : 1,
            "song" : [ {
            "name" : "Engel",
            "info" : "Piano version; performed on B-Stage",
            "with" : {
            "mbid" : "1e9d02a2-468a-4a53-ba46-6dbcd87b9595",
            "name" : "ABÉLARD",
            "sortName" : "Abélard",
            "disambiguation" : "French piano duo",
            "url" : "https://www.setlist.fm/setlists/abelard-53f78335.html"
            }
            }, {
            "name" : "Ausländer"
            }, {
            "name" : "Du riechst so gut"
            }, {
            "name" : "Pussy"
            }, {
            "name" : "Ich will"
            } ]
            }, {
            "encore" : 2,
            "song" : [ {
            "name" : "Rammstein"
            }, {
            "name" : "Adieu"
            }, {
            "name" : "Sonne",
            "tape" : true,
            "info" : "Piano version"
            }, {
            "name" : "Haifisch",
            "tape" : true,
            "info" : "Haiswing Remix by Olsen Involtini"
            }, {
            "name" : "Lügen",
            "tape" : true,
            "info" : "Instrumental"
            } ]
            } ]
            },
            "info" : "Final show of the Stadium Tour as a whole.",
            "url" : "https://www.setlist.fm/setlist/rammstein/2024/veltins-arena-gelsenkirchen-germany-63ab8613.html"
            }
            """;
        _httpMessageHandlerMock
            .When(HttpMethod.Get, "https://api.setlist.fm/rest/1.0/setlist/" + setlistId)
            .WithHeaders("x-api-key", _settingsMock.Object.Value.ClientSecret)
            .WithHeaders("Accept", "application/json")
            .Respond(HttpStatusCode.OK, "application/json", responseJson);

        // Act
        var (setlist, httpStatusCode) = await _sut.GetSetlistAsync(setlistId);

        // Assert
        Assert.NotNull(setlist);
        Assert.Equivalent(expectedSetlist, setlist);
        Assert.Equal(HttpStatusCode.OK, httpStatusCode);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Information, $"Successfully retrieved setlist with ID: {setlistId}");
    }

    [Fact]
    public async Task GetSetlistAsync_InvalidSecret_ReturnsNullAnd502()
    {
        // Arrange
        var setlistId = "validId";
        var responseJson = """
            {
              "message": "Forbidden"
            }
            """;
        _httpMessageHandlerMock
            .When(HttpMethod.Get, "https://api.setlist.fm/rest/1.0/setlist/" + setlistId)
            .Respond(HttpStatusCode.BadGateway, "application/json", responseJson);

        // Act
        var (setlist, httpStatusCode) = await _sut.GetSetlistAsync(setlistId);

        // Assert
        Assert.Null(setlist);
        Assert.Equal(HttpStatusCode.BadGateway, httpStatusCode);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Error, "Failed to retrieve the setlist");
    }

    [Fact]
    public async Task GetSetlistAsync_InvalidSetlistId_ReturnsNullAnd404()
    {
        // Arrange
        var setlistId = "invalidId";
        var responseJson = """
            {
              "code": 404,
              "status": "Not Found",
              "message": "not found",
            }
            """;
        _httpMessageHandlerMock
            .When(HttpMethod.Get, "https://api.setlist.fm/rest/1.0/setlist/" + setlistId)
            .Respond(HttpStatusCode.NotFound, "application/json", responseJson);

        // Act
        var (setlist, httpStatusCode) = await _sut.GetSetlistAsync(setlistId);

        // Assert
        Assert.Null(setlist);
        Assert.Equal(HttpStatusCode.NotFound, httpStatusCode);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Warning, "Not Found");
    }

    [Fact]
    public async Task GetSetlistAsync_InvalidResponseJson_ReturnsNullAnd502()
    {
        // Arrange
        var setlistId = "validId";
        var responseJson = "this is not valid json {";
        _httpMessageHandlerMock
            .When(HttpMethod.Get, "https://api.setlist.fm/rest/1.0/setlist/" + setlistId)
            .Respond(HttpStatusCode.OK, "application/json", responseJson);

        // Act
        var (setlist, httpStatusCode) = await _sut.GetSetlistAsync(setlistId);

        // Assert
        Assert.Null(setlist);
        Assert.Equal(HttpStatusCode.BadGateway, httpStatusCode);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Warning, "Failed to deserialize the setlist response", typeof(JsonException));
    }

    [Fact]
    public async Task GetSetlistAsync_NetworkErrorOccurs_ReturnsNullAnd502()
    {
        // Arrange
        var setlistId = "validId";
        _httpMessageHandlerMock
            .When(HttpMethod.Get, "https://api.setlist.fm/rest/1.0/setlist/" + setlistId)
            .Throw(new HttpRequestException("Simulated network failure."));

        // Act
        var (setlist, httpStatusCode) = await _sut.GetSetlistAsync(setlistId);

        // Assert
        Assert.Null(setlist);
        Assert.Equal(HttpStatusCode.BadGateway, httpStatusCode);

        // Verify logging
        _loggerMock.VerifyLog(LogLevel.Error, "A network error occurred", typeof(HttpRequestException));
    }

    #endregion
}
