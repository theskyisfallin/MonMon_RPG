using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// savable interface needed to make things savable
public interface ISavable
{
    object CaptureState();
    void RestoreState(object state);
}
