
using System.Collections.Generic;
using Nop.Plugin.Payments.MollieForNop.Models;

namespace Nop.Plugin.Payments.MollieForNop
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
            _identifiers.Clear();
        }

        public static void Remove(Identifier id) => _identifiers.RemoveAll(i => i.OrderInfo.Id == id.OrderInfo.Id);

    }

}