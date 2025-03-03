interface UserNameSettingFormProps {
    className?: string;
    allowCancel?: boolean;
    onSubmit?: (userName: string) => void;
    onClose?: () => void;
}

export default function UserNameSettingForm({ className, allowCancel, onSubmit } : UserNameSettingFormProps) {  
    function onFormSubmit(formData: FormData) {
        if (onSubmit) {
            const userName = formData.get("user_name")?.toString();
            if (userName) {
                onSubmit(userName);
            }
        }
    }

    return (
        <form className={`w-128 bg-white p-6 rounded-lg shadow-lg ${className}`} action={onFormSubmit}>
            <h3 className="text-lg font-bold mb-4 text-center">Set user name</h3>
            <input type="text" name="user_name" className="input input-bordered validator w-full" placeholder="User name"
                required={true} minLength={1} maxLength={32} pattern="[a-zA-Z0-9_]{1,32}" title="Only latin letters and digits allowed" />
            <p className="validator-hint text-xs text-error ml-1 mt-1 mb-0">
                Only latin letters and digits allowed
            </p>
            <div className="flex justify-center gap-8 mt-3">
                <button className="btn btn-primary w-24">OK</button>
                { allowCancel ? <button className="btn btn-secondary w-24">Cancel</button> : <></> }
            </div>
        </form>
    )
}