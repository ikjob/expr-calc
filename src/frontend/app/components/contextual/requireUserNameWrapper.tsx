import { useSelector } from 'react-redux';
import { Navigate, Outlet, useLocation } from 'react-router'
import { iseActiveUserNameSet } from '../../redux/stores/userStore';

export default function RequireUserNameWrapper() {
    const isActiveUserSet = useSelector(iseActiveUserNameSet);
    const location = useLocation();

    if (isActiveUserSet) {
        return <Outlet />;
    } 
    else 
    {
        return <Navigate state={{ from: location }} to="/login" replace={true} />;
    }
}