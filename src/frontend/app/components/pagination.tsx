export interface PaginationProps {
    activePage: number;
    pageSize: number;
    totalItemsCount: number;
    onPageChange: (pageNumber: number) => void;
}

export default function Pagination(props: PaginationProps) {  
    let pageCount = props.pageSize > 0 ? Math.floor((props.totalItemsCount - 1) / props.pageSize) + 1 : 1;
    if (pageCount == 0) {
        pageCount = 1;
    }

    const activePage = props.activePage < 1 ? 1 :(props.activePage > pageCount ? pageCount : props.activePage);
    let pages = [];
    if (pageCount <= 7) {
        pages = new Array(pageCount);
        for (let i = 0; i < pageCount; i++) {
            pages[i] = i + 1;
        }
    } else {
        if (activePage < 4) {
            pages = [1, 2, 3, 4, 5, -1, pageCount]
        } 
        else if (pageCount - activePage < 4) {
            pages = [1, -1, pageCount - 4, pageCount - 3, pageCount - 2, pageCount - 1, pageCount]
        } 
        else {
            pages = [1, -1, activePage - 1, activePage, activePage + 1, -1, pageCount]
        }
    }
    

    return (
        <div className="join">
            { pages.map((val, index) => val > 0 ? (
                <input key={index} className="join-item btn btn-square" type="radio" name="options" aria-label={val} defaultChecked={val == 1} onClick={() => props.onPageChange(val)} />
            ): (
                <input key={index} className="join-item btn btn-square btn-disabled" type="radio" name="options" aria-label="..." />
            ))}
        </div>
    )
}