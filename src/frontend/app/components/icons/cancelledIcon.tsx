import { memo } from "react";

function CancelledIcon({ className } : { className: string }) {
    return (
        <svg className={className} viewBox="0 0 32 32">
            <path d="M30 21.41L28.59 20L25 23.59L21.41 20L20 21.41L23.59 25L20 28.59L21.41 30L25 26.41L28.59 30L30 28.59L26.41 25L30 21.41z" fill="currentColor"></path><path d="M14 26a12 12 0 0 1 0-24z" fill="currentColor"></path><path d="M17.826 4.764a10.029 10.029 0 0 1 3.242 2.168L22.48 5.52a12.036 12.036 0 0 0-3.89-2.602z" fill="currentColor"></path><path d="M26 14a11.93 11.93 0 0 0-.917-4.59l-1.847.764A9.943 9.943 0 0 1 24 14z" fill="currentColor"></path>
        </svg>
    )
}

export default memo(CancelledIcon);