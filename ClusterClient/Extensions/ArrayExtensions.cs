using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterClient.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] Shuffle<T>(this T[] array)
        {
            var shuffledArray = array.Select(i => i).ToArray();
            var random = new Random();
            for (int i = array.Length - 1; i >= 1; i--)
            {
                int j = random.Next(i + 1);
                var temp = array[j];
                shuffledArray[j] = array[i];
                shuffledArray[i] = temp;
            }

            return shuffledArray;
        }
    }
}
