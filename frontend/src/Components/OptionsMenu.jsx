import './OptionsMenu.css'
import {saveAs} from 'file-saver'

function OptionsMenu({isToggled,positionX, positionY, file, openRenameMenu, updateFiles}){
    return(
        <menu className={isToggled ? 'menu active' : 'menu'} style={{top:positionY-5,left:positionX}}>
            <button className='button' onClick={() => saveAs("http://localhost:5173"+file.replace("/api","/api/download"))}>Download</button>
            <hr className='divider'></hr>
            <button className='button' onClick={() => openRenameMenu(file)}>Rename</button>
            <hr className='divider'></hr>
            <button className='button' onClick={async () => {
                const requestOptions = {
                    method: 'DELETE'
                }

                const res = await fetch(file, requestOptions);
                updateFiles();

            }}>Delete</button>
            
        </menu>
    );
}

export default OptionsMenu