using Astrare.Translate;

namespace Astrare.Models;

public class Object
{
    public ObjectIdentifier identifier { get; set; }
    public ObjectType type { get; set; }
    public float? radius { get; set; }

    public override string ToString()
    {
        return Language.Current.Translate(identifier.ToString().ToLower().FirstCharToUpper());
    }
}