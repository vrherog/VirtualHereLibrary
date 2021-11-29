package VirtualHereLibrary

import (
	"bufio"
	"fmt"
	"os"
	"sync"
)

type WindowsClient struct {
	sync.Mutex
	//cache sync.Map
}

func (c *WindowsClient) Query(command string) (result string, err error) {
	//var cacheKey = fmt.Sprintf("WindowsClient:Query:%s", command)
	//if _, ok := c.cache.LoadOrStore(cacheKey, 0); ok {
	//	return
	//}
	c.Lock()
	var pipeName = fmt.Sprintf(`\\.\pipe\%s`, DefaultPipeName)
	var pipe *os.File
	if pipe, err = os.OpenFile(pipeName, os.O_RDWR, os.ModeNamedPipe); err == nil {
		defer pipe.Close()
		var n int
		if n, err = pipe.WriteString(command); err == nil && n > 0 {
			var reader = bufio.NewReader(pipe)
			var buffer = make([]byte, 81920)
			if n, err = reader.Read(buffer); err == nil {
				result = string(buffer[:n])
			}
		}
	}
	//c.cache.Delete(cacheKey)
	c.Unlock()
	return
}
