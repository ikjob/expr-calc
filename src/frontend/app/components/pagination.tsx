export default function Pagination() {  
  return (
      <div className="join">
        <input
          className="join-item btn btn-square"
          type="radio"
          name="options"
          aria-label="1"
          defaultChecked />
        <input className="join-item btn btn-square" type="radio" name="options" aria-label="2" />
        <input className="join-item btn btn-square" type="radio" name="options" aria-label="3" />
        <input className="join-item btn btn-square btn-disabled text-normal" type="radio" name="options" aria-label="..." />
        <input className="join-item btn btn-square" type="radio" name="options" aria-label="5" />
        <input className="join-item btn btn-square" type="radio" name="options" aria-label="6" />
      </div>
  )
}