import './File.css'
import {useState} from 'react'

function File({reference,type,byteSize,lastUpdated,name}){

    return(
        <div className='file'>
            <img src='/paper.png' className='file_icon'></img>
            <div className='filename'>{name}</div>
        </div>
    );
}

export default File