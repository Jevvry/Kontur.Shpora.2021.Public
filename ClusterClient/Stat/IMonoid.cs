using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterClient.Stat
{
    public interface IMonoid<T>
    {
        T Sum(T other);
        T Subtraction(T other);
    }
}
