import './Header.css'
import Logout from './Logout.jsx';
import Path from './Path.jsx'

function Header({path, updatePath}){
    return(
        <div className="header">
            <Path path={path} updatePath={updatePath}></Path>
            <Logout></Logout>
        </div>
    );
}

export default Header