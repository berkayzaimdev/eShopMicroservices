namespace BuildingBlocks.Abstractions
{
    public interface IEntityBuilder<in TDto, out TEntity>
    {
        TEntity Build(TDto dto);
    }
}
