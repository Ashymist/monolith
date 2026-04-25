import './RenameMenu.css'
import {useState} from "react";

function RenameMenu({isToggled, fileReference, closeRenameMenu, updateFiles}){
    const [newName, setNewName] = useState("");

    return(
        <div className={isToggled ? "rename-menu active" : "rename-menu"}>
            <div className='rename-menu-prompt'>Enter new name:</div>
            <form>
                <input className='newNameInputField' value={newName} onChange={e => setNewName(e.target.value)}></input>
            </form>
            <div className='button-container'>
                <button className='rename-button' onClick={ async () => {
                    const requestOptions = {
                        method: 'PATCH',
                    }

                    const res = await fetch(`http://localhost:5173${fileReference}?newname=${newName}`, requestOptions);
                    updateFiles();
                    closeRenameMenu();
                }}>Confirm</button>
                <button className='rename-button' onClick={closeRenameMenu}>Cancel</button>
            </div>
        </div>
    );
}

export default RenameMenu