namespace BookHive.Domain.DTOS;

public record GetFilteredDto(
    int Skip,
    int PageSize,
    string SearchValue,
    string SortColumnIndex,
    string SortColumn,
    string SortColumnDirection
);
