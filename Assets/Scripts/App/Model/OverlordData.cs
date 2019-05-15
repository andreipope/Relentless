using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;

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
