import './Path.css'
import PathPart from './PathPart.jsx'

function Path({path,updatePath}){
    console.log(path);
    const parts = path.split('/').filter((word) =>  word != "");
    console.log(parts);
    return(
        <>
            <div className='path'>
                /
                {parts.map(part => {
                return(<PathPart part={part} currentPath={path} updatePath={updatePath} key={path.substring(0,path.indexOf(part)+part.length)}/>);
                })
                }
            </div>
            
        </>  
    )
}
export default Path