using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
  interface ICommunicate
  {
    bool Start();
    bool Stop();
    bool Send(List<byte> arr);
    bool Send(string s);
    bool Ready();
    void UpdateValues(object sender, AudioAvailableEventArgs e);
  }
}
