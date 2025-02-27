import { useState } from 'react'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
        <button onClick={() => setCount((count) => count + 1)} className='btn btn-primary'>
          count is {count}
        </button>
    </>
  )
}

export default App
