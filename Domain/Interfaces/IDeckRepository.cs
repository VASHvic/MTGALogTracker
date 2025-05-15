using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IDeckRepository
    {
        Task AddDecksAsync(List<Deck> decks);
        Task<List<Deck>> GetDecksByIds(List<string> deckIds);
        Task<Deck?> GetDeckById(string deckId);
    }
}
