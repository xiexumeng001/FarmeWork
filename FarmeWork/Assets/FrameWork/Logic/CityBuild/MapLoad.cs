using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShipFarmeWork.Resource;

namespace ShipFarmeWork.Logic.CityMap
{
    public static class MapLoad
    {

        public static TextAsset LoadMapData(string mapPath, string mapName)
        {
            TextAsset mapJson = (TextAsset)ResHelp.GetAsset(mapPath, mapName + ".json", typeof(TextAsset), ResHelp.CommonGame);
            return mapJson;
        }

    }

}