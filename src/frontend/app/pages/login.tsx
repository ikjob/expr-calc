import { useNavigate } from "react-router";
import UserNameSettingForm from "../components/userNameSettingForm";
import { useAppDispatch } from "../redux/hooks";
import { setActiveUserName } from "../redux/stores/userStore";

export default function LoginPage() {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();

    function onSubmitUserName(userName: string) {
        dispatch(setActiveUserName(userName));
        navigate("/", { replace: true });
    }

    return (
        <div className="bg-base-300 flex items-center justify-center min-h-screen">
            <UserNameSettingForm className="relative" allowCancel={false} onSubmit={onSubmitUserName} />
        </div>
    )
}