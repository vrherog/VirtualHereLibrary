package VirtualHereLibrary

import (
	"io/ioutil"
	"os"
	"sync"
)

type NonWindowsClient struct {
	sync.Mutex
	//cache  sync.Map
}

func (c *NonWindowsClient) Query(command string) (result string, err error) {
	//var cacheKey = fmt.Sprintf("NonWindowsClient:Query:%s", command)
	//if _, ok := c.cache.LoadOrStore(cacheKey, 0); ok {
	//	return
	//}
	c.Lock()
	var info os.FileInfo
	if info, err = os.Stat(DefaultInputStreamFile); err == nil || os.IsExist(err) {
		if err = ioutil.WriteFile(DefaultInputStreamFile, []byte(command), info.Mode()); err == nil {
			if buffer, err := ioutil.ReadFile(DefaultOutputStreamFile); err == nil {
				result = string(buffer)
			}
		}
	}
	//c.cache.Delete(cacheKey)
	c.Unlock()
	return
}
