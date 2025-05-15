using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DeckRepository(ApplicationDbContext context) : IDeckRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task AddDecksAsync(List<Deck> decks)
        {
            if (decks == null || decks.Count == 0)
            {
                return;
            }

            var uniqueDecks = decks
                .GroupBy(d => d.DeckId)
                .Select(g => g.Last())
                .ToList();

            var deckIds = uniqueDecks.Select(d => d.DeckId).ToList();
            var existingDeckIds = await _context.Decks
                .Where(d => deckIds.Contains(d.DeckId))
                .Select(d => d.DeckId)
                .ToListAsync();

            foreach (var deck in uniqueDecks)
            {
                if (existingDeckIds.Contains(deck.DeckId))
                {
                    _context.Decks.Update(deck);
                }
                else
                {
                    _context.Decks.Add(deck);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Deck?> GetDeckById(string deckId)
        {
            return await _context.Decks.FirstOrDefaultAsync(d => d.DeckId == deckId);
        }

        public async Task<List<Deck>> GetDecksByIds(List<string> deckIds)
        {
            return await _context.Decks.Where(d => deckIds.Contains(d.DeckId)).ToListAsync();
        }
    }
}
