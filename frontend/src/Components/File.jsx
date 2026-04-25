import './File.css'
import {useState} from 'react'
import OptionsMenu from './OptionsMenu.jsx'

function File({reference,type,byteSize,lastUpdated,name,contextMenuHandler}){

    return(
        <div className='file' onContextMenu={(e) => contextMenuHandler(e,reference)}>
            <img src='/02_Document_48x48.webp' className='file_icon'></img>
            <div className='filename'>{name}</div>
        </div>
    );
}

export default File