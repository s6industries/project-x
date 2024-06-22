
class Camera:

    def __init__(self, toFocus, screen):
        self.toFocus = toFocus # the character or other
        self.screen = screen # the width and the height of the screen

    # camera will follow the player
    def move(self, otherObjects):
        if self.toFocus.left:
            if self.toFocus.rect.x <= ((self.screen['width'] - self.toFocus.width) // 2):
                for obj in otherObjects:
                    if obj.name in ['zombies', 'robots', 'slime']:
                        obj.rect.x += self.toFocus.speed
                    else:
                        obj.move_x(self.toFocus.speed)
        elif self.toFocus.right:
            if self.toFocus.rect.x >= ((self.screen['width'] - self.toFocus.width) // 2):
                for obj in otherObjects:
                    if obj.name in ['zombies', 'robots', 'slime']:
                        obj.rect.x += self.toFocus.speed * -1
                    else:
                        obj.move_x((self.toFocus.speed * -1))
        elif self.toFocus.up:
            if self.toFocus.rect.y <= ((self.screen['height'] - self.toFocus.height) // 2):
                for obj in otherObjects:
                    if obj.name in ['zombies', 'robots', 'slime']:
                        obj.rect.y += self.toFocus.speed
                    else:
                        obj.move_y(self.toFocus.speed)
        elif self.toFocus.down:
            if self.toFocus.rect.y >= ((self.screen['height'] - self.toFocus.height) // 2):
                for obj in otherObjects:
                    if obj.name in ['zombies', 'robots', 'slime']:
                        obj.rect.y += self.toFocus.speed * -1
                    else:
                        obj.move_y((self.toFocus.speed * -1))