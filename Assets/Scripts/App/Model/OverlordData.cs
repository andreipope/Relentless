using System;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground.Data
{
    public class OverlordData
    {
        public List<OverlordModel> Overlords { get; }

        public OverlordData(List<OverlordModel> overlords)
        {
            Overlords = overlords ?? throw new ArgumentNullException(nameof(overlords));
        }

        public OverlordModel GetOverlordById(OverlordId id)
        {
            return Overlords.Single(model => model.Id == id);
        }
    }

}
