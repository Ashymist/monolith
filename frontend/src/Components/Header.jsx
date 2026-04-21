import './Header.css'
import Logout from './Logout.jsx';
import Path from './Path.jsx'

function Header(props){
    return(
        <div className="header">
            <Path path={props.path}></Path>
            <Logout></Logout>
        </div>
    );
}

export default Header