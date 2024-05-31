using System.Collections.Generic;
using CessilCellsCeaChells.CeaChore;
using UnityEngine;

[assembly: RequiresField(typeof(Item), "modGuid", typeof(string))]
[assembly: RequiresField(typeof(Item), "spawnableIn", typeof(List<string>))]
[assembly: RequiresField(typeof(Item), "useInGame", typeof(bool))]