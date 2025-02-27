namespace ZenGarden.Domain.DTOs;

public class FilterResult<T>(List<T> data, int totalCount)
    where T : class
{
    public int TotalCount { get; set; } = totalCount;
    public List<T> Data { get; set; } = data;
}


