import { useState } from "react";
import { ErrorDetails, isErrorDetails } from "../models/errorDetails";

export type AlertBarSeverity = "info" | "warn" | "error";
export type AlertContent = {
    title: string;
    detail?: string | null;
}
export type AlertItem = {
    severity: AlertBarSeverity;
    value: AlertContent | string | ErrorDetails | unknown;
    key?: React.Key;
}

function isAlertContent(rawItem: unknown) : rawItem is AlertContent {
    if (typeof rawItem === "object") {
        const obj = rawItem as object;
        return obj != null &&
            "title" in obj &&
            typeof obj.title === "string" &&
            obj.title != null;
    }
    return false;
}


export interface AlertBarProps {
    items: AlertItem[];
    itemToAlertContentConverter?: (item: AlertItem) => AlertContent | null;
}

function defaultItemValueConverter(item: AlertItem) : AlertContent {
    const itemValue = item.value;
    
    if (isErrorDetails(itemValue)) {
        return {
            title: itemValue.title ?? "Error",
            detail: itemValue.detail
        }
    }
    else if (isAlertContent(itemValue)) {
        return itemValue;
    }
    else if (typeof itemValue === "string") {
        const title = (item.severity == "error" ? "Error" : 
                    (item.severity == "warn" ? "Warning": 
                    (item.severity == "info" ? "Info" : "Error")))
        return {
            title: title,
            detail: itemValue
        }
    }
    else 
    {
        return {
            title: "Internal error"
        };
    }
}


export default function AlertBar(props: AlertBarProps) {  
    const [closedItems, setClosedItems] = useState(new Map<React.Key, boolean>());

    if (props.items.length == 0) {
        return <></>
    }

    function itemClass(severity: AlertBarSeverity) {
        switch (severity) {
            case "error":
                return "alert-error";
            case "warn":
                return "alert-warning";
            case "info":
                return "alert-info";
            default:
                return "alert-error";
        }
    }

    function onCloseItem(key: React.Key) {
        if (!closedItems.get(key)) {
            setClosedItems((oldMap) => {
                const newMap = new Map<React.Key, boolean>();
                for (let index = 0; index < props.items.length; index++) {
                    const key = props.items[index].key ?? index;
                    if (oldMap.get(key)) {
                        newMap.set(key, true);
                    }
                }
                newMap.set(key, true);
                return newMap;
            });
        }
    }

    return (
        <div className="w-1/2 fixed bottom-0 left-1/2 -translate-x-1/2">
        { props.items.map((item, index) => {
            const itemKey = item.key ?? index;
            const itemValue = props.itemToAlertContentConverter ? props.itemToAlertContentConverter(item) : defaultItemValueConverter(item);

            if (itemValue == null || closedItems.get(itemKey)) {
                return <span key={itemKey} />;
            }
            
            return (
                <div key={itemKey} role="alert" className={`alert ${itemClass(item.severity)}`}>
                    <span className="font-bold">{itemValue.title}{itemValue.detail ? ": " : ""}</span>
                    {itemValue.detail ? <span>{itemValue.detail}</span> : <span />}
                    <div><button className="btn btn-square btn-ghost bg-base-content/5 btn-xs" onClick={() => onCloseItem(itemKey)}>x</button></div>
                </div>)
        })}
        </div>
    )
}