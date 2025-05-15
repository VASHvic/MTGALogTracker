using Domain.Interfaces;
using Domain.Models;
using Domain.Models.Deck;
using Domain.Entities;
using System.Text.Json;

namespace LogWorker.Services.CoreServices
{
    public class DeckService : IDeckService
    {
        private readonly ILogger<DeckService> _logger;
        private readonly IDeckRepository _deckRepository;

        public DeckService(ILogger<DeckService> logger, IDeckRepository deckRepository)
        {
            _logger = logger;
            _deckRepository = deckRepository;
        }

        public string FetchDeck(string line, StreamReader sr, string delimeter, EventState eventState)
        {
            if (line.Contains("<== EventSetDeckV2"))
            {
                var json = sr.ReadLine();
                if (json == null)
                {
                    _logger.LogError("Failed to read deck details from stream.");
                    return string.Empty;
                }

                try
                {
                    var deckDetails = JsonSerializer.Deserialize<EventSetDeckV2Dto>(json);
                    if (deckDetails?.CourseDeckSummary != null)
                    {
                        var deckId = deckDetails.CourseDeckSummary.DeckId;
                        var deckName = deckDetails.CourseDeckSummary.Name;
                        if (!string.IsNullOrEmpty(deckId) && !string.IsNullOrEmpty(deckName))
                        {
                            eventState.UpdateEventStateDeck(deckId, deckName);
                        }
                    }
                }
                catch (Exception)
                {
                    _logger.LogError("Failed to deserialize deck details in order to assign current deck.");
                }
                return json + delimeter;
            }
            return string.Empty;
        }

        public async Task WriteDecks(List<EventSetDeckV2Dto> decksInfo)
        {
            if (decksInfo == null || decksInfo.Count == 0)
            {
                _logger.LogInformation("No decks to write.");
                return;
            }

            var decks = new List<Deck>();

            foreach (var deckInfo in decksInfo)
            {
                if (deckInfo.CourseDeckSummary == null ||
                    deckInfo.CourseDeck == null ||
                    string.IsNullOrEmpty(deckInfo.CourseDeckSummary.DeckId))
                {
                    _logger.LogWarning("Skipping deck with missing or invalid deck ID");
                    continue;
                }

                var deck = new Deck
                {
                    DeckId = deckInfo.CourseDeckSummary.DeckId,
                    Name = deckInfo.CourseDeckSummary.Name,
                    MainDeckJson = JsonSerializer.Serialize(deckInfo.CourseDeck.MainDeck),
                    SideboardJson = deckInfo.CourseDeck.Sideboard?.Any() == true ?
                        JsonSerializer.Serialize(deckInfo.CourseDeck.Sideboard) : null
                };

                decks.Add(deck);
            }

            if (decks.Any())
            {
                try
                {
                    await _deckRepository.AddDecksAsync(decks);
                    _logger.LogInformation($"Successfully wrote {decks.Count} decks to the database");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to write decks to database");
                    throw;
                }
            }
        }
    }
}
