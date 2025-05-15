using Domain.Models;
using Domain.Models.Deck;

namespace Domain.Interfaces
{
    public interface IDeckService
    {
        string FetchDeck(string line, StreamReader sr, string delimeter, EventState eventState);
        Task WriteDecks(List<EventSetDeckV2Dto> decksInfo);
    }
}
