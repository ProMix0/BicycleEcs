using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicycleEcs
{

    public interface IEcsSystem
    {
    }

    public interface IEcsInitSystem : IEcsSystem
    {
        void Init(IEcsWorld world);
    }

    public interface IEcsRunSystem : IEcsSystem
    {
        void Run();
    }


    public interface IEcsDestroySystem : IEcsSystem
    {
        void Destroy(IEcsWorld world);
    }
}
