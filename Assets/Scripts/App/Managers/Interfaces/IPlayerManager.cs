using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public interface IPlayerManager
    {
        User LocalUser { get; set; }
    }
}