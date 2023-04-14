using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AillieoUtils.UniPy
{
    public class GRPCSettings : ScriptableObject
    {
        public int port;

        public string pythonScript;

        public string protoPath;
        public string genCSProtoPath;
        public string genPythonProtoPath;
    }
}
