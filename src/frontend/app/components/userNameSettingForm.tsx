import { useEffect, useRef } from "react";
import { useSelector } from "react-redux";
import { selectActiveUserName, setActiveUserName } from "../redux/stores/userStore";
import { useAppDispatch } from "../redux/hooks";

interface UserNameSettingFormProps {
    className?: string;
    allowCancel?: boolean;
    defaultValue?: string;
    onSubmit?: (userName: string) => void;
    onCancel?: () => void;
}

export default function UserNameSettingForm({ className, allowCancel, defaultValue, onSubmit, onCancel } : UserNameSettingFormProps) {  
    function onFormSubmit(formData: FormData) {
        if (onSubmit) {
            const userName = formData.get("user_name")?.toString();
            if (userName) {
                onSubmit(userName);
            }
        }
    }

    function onFormCancel() {
        if (onCancel) {
            onCancel();
        }
    }

    return (
        <form className={`w-128 bg-base-100 border-1 border-neutral-content/10 p-6 rounded-lg shadow-lg ${className}`} action={onFormSubmit}>
            <h3 className="text-lg font-bold mb-4 text-center">Set user name</h3>
            <input type="text" name="user_name" className="input input-bordered validator w-full" defaultValue={defaultValue} placeholder="User name"
                required={true} minLength={1} maxLength={32} pattern="[a-zA-Z0-9_]{1,32}" title="Only latin letters and digits allowed" />
            <p className="validator-hint text-xs text-error ml-1 mt-1 mb-0">
                Only latin letters and digits allowed
            </p>
            <div className="flex justify-center gap-8 mt-3">
                <button type="submit" className="btn btn-primary w-24">OK</button>
                { allowCancel ? <button type="button" className="btn btn-secondary w-24" onClick={onFormCancel}>Cancel</button> : <></> }
            </div>
        </form>
    )
}


interface UserNameSettingModalProps {
    isOpen: boolean;
    onClose: () => void;
}

export function UserNameSettingModal({ isOpen, onClose } : UserNameSettingModalProps) {
    const dialogRef = useRef<HTMLDialogElement>(null);
    const showCounter = useRef(0);
    const activeUser = useSelector(selectActiveUserName);
    const dispatch = useAppDispatch();

    useEffect(() => {
        if (dialogRef.current?.open && !isOpen) {
            dialogRef.current?.close()
        } else if (!dialogRef.current?.open && isOpen) {
            showCounter.current = showCounter.current + 1;
            dialogRef.current?.showModal()
        }
    }, [isOpen])

    function onModalClose() {
        if (dialogRef.current?.open) {
            dialogRef.current?.close()
        }
        onClose();
    }

    function onSetUserName(userName: string) {
        if (userName) {
            dispatch(setActiveUserName(userName));
            onModalClose();
        }
    }
    
    return (
        <dialog className="modal" ref={dialogRef} onClose={onModalClose}>
            <UserNameSettingForm key={showCounter.current} className="modal-box" allowCancel={true} defaultValue={activeUser ?? ""} onSubmit={onSetUserName} onCancel={onModalClose} />
        </dialog>
    )
}