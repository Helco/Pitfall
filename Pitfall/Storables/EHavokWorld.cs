using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pitfall.Havok;

namespace Pitfall.Storables;

[StorableType]
public class EHavokWorld : EStorable
{
    public HavokWorld World { get; private set; } = null!;

    public override void Read(BinaryReader reader) => World = new(reader);
}
