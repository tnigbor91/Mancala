using SS.Mancala.BL;
using SS.Mancala.BL.Models;

public class PitManager : GenericManager<tblPit>
{
    public PitManager(ILogger logger, DbContextOptions<MancalaEntities> options) : base(logger, options) { }
    public PitManager(DbContextOptions<MancalaEntities> options) : base(options) { }
    public PitManager() { }

    public async Task<Guid> InsertAsync(Pit pit, bool rollback = false)
    {
        try
        { 
            tblPit row = new tblPit
            {
                Id = pit.Id,
                PitPosition = pit.PitPosition,
                Stones = pit.Stones,
                IsMancala = pit.IsMancala, 
                PlayerId = pit.PlayerId  
            };

            return await base.InsertAsync(row, null, rollback);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting pit: {ex.Message}", ex);
        }
    }




    public async Task<int> UpdateAsync(Pit pit, bool rollback = false)
    {
        try
        {
            tblPit row = Map<Pit, tblPit>(pit);
            return await base.UpdateAsync(row, rollback);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating pit: {ex.Message}", ex);
        }
    }

    public async Task<List<Pit>> LoadAsync()
    {
        try
        {
            var tblPits = await base.LoadAsync();
            return tblPits.Select(e => Map<tblPit, Pit>(e)).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading pits: {ex.Message}", ex);
        }
    }


    public async Task<Pit> LoadByIdAsync(Guid id)
    {
        try
        {
            var rows = await base.LoadAsync(e => e.Id == id);
            if (rows != null && rows.Any())
            {
                return Map<tblPit, Pit>(rows.First());
            }
            else
            {
                throw new Exception($"Pit with ID {id} not found.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading pit by ID: {ex.Message}", ex);
        }
    }




}
