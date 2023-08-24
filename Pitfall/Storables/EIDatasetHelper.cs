using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

// data seems to be entirely ignored by the game
// paths to source files, references also present for a GC Sims game

[StorableType]
public class EIDatasetHelper : EIGameInstance
{
    public (string, string) strPair1;
    public (string, string) strPair2;
    public (string, string) strPair3;
    public (string, string) strPair4;
    public (string, string) strPair5;
    public (string, string) strPair6;
    public (string, string) strPair7;
    public uint i1;

    public override void ReadInstanceData(BinaryReader reader)
    {
        strPair1 = (reader.ReadCString(), reader.ReadCString());
        strPair2 = (reader.ReadCString(), reader.ReadCString());
        strPair3 = (reader.ReadCString(), reader.ReadCString());
        strPair4 = (reader.ReadCString(), reader.ReadCString());
        strPair5 = (reader.ReadCString(), reader.ReadCString());
        strPair6 = (reader.ReadCString(), reader.ReadCString());
        strPair7 = (reader.ReadCString(), reader.ReadCString());
        i1 = reader.ReadUInt32();
    }
}
