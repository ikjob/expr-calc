export default function UserNameModal() {  
  return (
    <div className="w-128 bg-white p-6 rounded-lg shadow-lg text-center">
        <h3 className="text-lg font-bold mb-4">Set user name</h3>
        <input type="text" className="input input-bordered w-full mb-4" placeholder="User name" />
        <div className="flex justify-center gap-4">
              <button className="btn btn-primary">OK</button>
              <button className="btn btn-secondary">Cancel</button>
        </div>
    </div>
  )
}