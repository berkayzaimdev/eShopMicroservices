using BuildingBlocks.Abstractions;
using System.Reflection;

namespace BuildingBlocks.Concretes
{
    public class EntityBuilder<TDto, TEntity> : IEntityBuilder<TDto, TEntity>
        where TDto : class
        where TEntity : class, new()
    {
        public TEntity Build(TDto dto)
        {
            TEntity entity = new();

            var entityProps = typeof(TEntity).GetProperties();
            var dtoProps = typeof(TDto).GetProperties();

            foreach(var prop in dtoProps)
            {

                var entityProp = Array.Find(entityProps, x => x.Name == prop.Name);

                if (entityProp != null)
                {
                    entityProp.SetValue(entity, prop.GetValue(dto));
                }
            }

            return entity;
        }
    }
}
