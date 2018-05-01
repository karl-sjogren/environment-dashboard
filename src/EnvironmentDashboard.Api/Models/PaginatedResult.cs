using System;
using System.Collections.Generic;

namespace EnvironmentDashboard.Api.Models {
    public class PaginatedResult<T> {
        public PaginatedResult(ICollection<T> items, Int32 pageIndex, Int32 pageSize, long totalCount) {
            Items = items;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public ICollection<T> Items { get; }
        public Int32 PageIndex { get; }
        public Int32 PageSize { get; }
        public long TotalCount { get; }
    }
}