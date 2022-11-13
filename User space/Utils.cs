using System.Reflection;

namespace BicycleEcs
{
    public static class Utils
    {
        public static void InjectPools(IPoolsList pools, object system)
        {
            foreach (var field in system.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.FieldType.IsAssignableTo(typeof(IComponentPool)) && !field.FieldType.Equals(typeof(IComponentPool)))
                {
                    IComponentPool pool = pools.GetComponentPool(field.FieldType.GenericTypeArguments.Single());
                    field.SetValue(system, pool);
                }
            }
        }
    }
}