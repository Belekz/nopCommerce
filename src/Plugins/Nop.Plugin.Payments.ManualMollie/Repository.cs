
using System.Collections.Generic;
using Nop.Plugin.Payments.ManualMollie.Models;

namespace Nop.Plugin.Payments.ManualMollie
{

    public static class Repository
    {

        private static List<Identifier> _identifiers = new List<Identifier>();


        public static IEnumerable<Identifier> Identifiers => _identifiers;


        public static void AddInfo(Identifier id)
        {
            _identifiers.Add(id);
        }

        public static void Reset()
        {
            _identifiers = null;
        }

    }

}