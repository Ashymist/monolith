import './File.css'
import {useState} from 'react'

function File({reference,type,byteSize,lastUpdated,name}){

    return(
        <div className='file'>
            <img src='/02_Document_48x48.webp' className='file_icon'></img>
            <div className='filename'>{name}</div>
        </div>
    );
}

export default File