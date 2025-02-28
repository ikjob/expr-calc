import { Calculator, UserCircle } from '@ricons/fa'

export default function Navbar() {
  return (
    <nav className="navbar bg-base-200">
      <div className="navbar-start">
        <div className="flex-1 ml-2">
          <Calculator className="h-[3em] text-xs text-base-content" />
        </div>
      </div>

      <div className="navbar-end">
        <UserLoginButton />
      </div>
    </nav>
    )
}


function UserLoginButton() {
    return (
      <div className="dropdown dropdown-end">
        <div tabIndex={0} className="btn btn-ghost rounded-btn" role="button">
          <a>User1</a>
          <UserCircle className="rounded-full h-[2em]" />
        </div>
        <ul 
          tabIndex={0}
          className="menu menu-sm dropdown-content bg-base-100 rounded-box z-[1] mt-3 w-52 p-2 shadow">
          <li><a>Change</a></li>
        </ul>  
      </div>
    )
  }