from .base import CZBTests
import time


class Gameplay_Page(CZBTests):
    
    def __init__(self,altdriver,driver):
        self.altdriver=altdriver
        self.driver=driver
        self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')
        self.end_turn_button=self.altdriver.wait_for_element('EndTurnButton/_1_btn_endturn')
        self.player_board=self.altdriver.wait_for_element('Battlefield/PlayerBoard')
        self.opponent_board=self.altdriver.wait_for_element('Battlefield/OpponentBoard')
        self.opponent_face=self.altdriver.wait_for_element('Opponent/OverlordArea/RegularModel/RegularPosition/Avatar/OverlordImage')
        self.player_primary_spell=self.altdriver.wait_for_element('Player/Object_SpellPrimary')
        self.player_secondary_spell=self.altdriver.wait_for_element('Player/Object_SpellSecondary')

    def get_cards_that_are_in_hand(self):
        cards=[]
        cards = self.altdriver.find_elements('CreatureCard(Clone)')
        cards.extend(self.altdriver.find_elements('ItemCard(Clone)'))
        cards.sort(key=lambda element:float(element.x))
        return cards

    def swipe_card_from_hand_to_board(self,cardPosition):
        cards=self.get_cards_that_are_in_hand()
        cardGoo = self.altdriver.wait_for_element(
            'id('+str(cards[cardPosition].id)+')/GooText')
        self.driver.swipe(int(cardGoo.x), int(cardGoo.mobileY),
                          int(self.player_board.x), int(self.player_board.mobileY), 2000)
        time.sleep(4)

    def get_player_board_creatures(self):
        cards=self.altdriver.find_elements('PlayerBoard/BoardCreature(Clone)')
        cards.sort(key=lambda element:float(element.x))
        return cards
    def get_opponent_board_creatures(self):
        cards=self.altdriver.find_elements('OpponentBoard/BoardCreature(Clone)')
        cards.sort(key=lambda element:float(element.x))
        return cards

    def end_turn_and_wait_for_your_turn(self):
        self.end_turn_button.mobile_tap()
        time.sleep(4)
        self.altdriver.wait_for_element('YourTurnPopup(Clone)')
        self.altdriver.wait_for_element('id('+str(self.end_turn_button.id)+')/EndTurnGlowEffect', timeout=60)
        time.sleep(5)#sleep is to wait for the card player draws go to player hand
    
    def swipe_board_card_to_opponent_face(self,cardPosition):
        player_board_creature=self.get_player_board_creatures()
        player_board_creature[cardPosition].mobile_dragToElement(self.opponent_face,2)
        time.sleep(4)

    def swipe_primary_spell_to_opponent_face(self):
        self.player_primary_spell.mobile_dragToElement(self.opponent_face,2)
        time.sleep(4)
    
    def swipe_primary_spell_to_opponent_creature(self,cardPosition):
        enemy_board_creature=self.get_opponent_board_creatures()
        self.player_primary_spell.mobile_dragToElement(enemy_board_creature[cardPosition],2)
        time.sleep(4)

    
    def swipe_board_card_to_opponent_creature(self,player_card_position,opponent_card_position):
        player_board_creature=self.get_player_board_creatures()
        enemy_board_creature=self.get_opponent_board_creatures()
        player_board_creature[player_card_position].mobile_dragToElement(enemy_board_creature[opponent_card_position],2)
        time.sleep(6)

    def play_player_turn():

        ##play buff cards/ utility
        ##action phase
        ##play if goo permits more cards
        ##end turn

    
    def action_phase():
        
        
        while self.attack_taunt_card():
            pass

        damaging_cards=self.get_every_possible_thing_that_can_cause_damage_this_turn()
        if len(damaging_cards)!=0:
            if self.is_lethal_available(damaging_cards) or len(self.get_opponent_board_creatures())==0:
                for damaging_card in damaging_cards:
                    self.attack_opponent_face
            else:
                while(self.trade_cards())
                    pass



    def calculate_card_score(player_card_attack,player_card_health,opponent_card_attack,opponent_card_health):
        if player_card_attack==opponent_card_health and player_card_health>opponent_card_attack:
            score=100-(player_card_health-opponent_card_attack)
            if score<90:
                score=90
            return score
        if player_card_attack>opponent_card_health and player_card_health>opponent_card_attack:
            score= 90-(player_card_attack-opponent_card_health)-(player_card_health-opponent_card_attack)
            if score<70:
                score=70
            return score
        if player_card_attack>=opponent_card_health and player_card_health<=opponent_card_attack:
            score=69+(opponent_card_health-player_card_attack)+(player_card_health-opponent_card_attack)
            if score<60:
                score=60
            return score
        if player_card_attack<opponent_card_health and player_card_health>opponent_card_attack:
            score=59-(opponent_card_health-player_card_attack)-(player_card_health-opponent_card_attack)
            if score<30:
                score=30
            return score
        if player_card_attack<opponent_card_attack and player_card_health<=opponent_card_attack:
            score=29-(opponent_card_health-player_card_attack)-(opponent_card_attack-player_card_health)
            if(score<0):
                score=0
            return score
        print("Error: situation not covered ",player_card_attack,player_card_health,opponent_card_attack,opponent_card_health)
        return -1
    
    def get_every_possible_thing_that_can_cause_damage_this_turn():
        ##Create list of tuple(altunityobject,type,name,attack,health,attacked)
        ##Add all active zombies from the board
        ##Add all zombies,spell than can do damage
        ##return the list
    def get_all_active_zombies_from_board():
        ##return every zombie that is not frozen or that attacked yet

    def get_all_damaging_cards_from_hand():
        ##return every card from hand that could do damage


    def get_all_opponent_heavy_cards():
        #TODO change name function
        ##Create list of tuple(altunityObject,name,attack,health,score)
        ##return all opponent taunts

    def is_lethal_available(available_damage):
        total_damage_available=0
        for element in available_damage:
            total_damage_available=total_damage_available+element[3]
        opponent_health=self.get_opponent_health()
        return total_damage_available>opponent_health
    
    def is_card_from_hand_able_to_damage_and_playable_this_turn(card):
        ##check card


    def attack_heavy_zombies():
        ##TODO change name function
        cards_able_to_do_damage=self.get_every_possible_thing_that_can_cause_damage_this_turn()
        taunts=self.get_all_opponent_heavy_cards()

        if len(cards_able_to_do_damage)==0 or len(taunts)==0:
            return False
        for damage_card in cards_able_to_do_damage:
            for taunt_card in taunts:
                score=self.calculate_card_score(damage_card[3],damage_card[4],taunt_card[2],taunt_card[3])
                if score<taunt_card[4]:
                    taunt_card[4]=score

        for taunt_card in taunts:
            for damage_card in cards_able_to_do_damage:
                score=self.calculate_card_score(damage_card[3],damage_card[4],taunt_card[2],taunt_card[3])
                if score==taunt_card[4]:
                    self.attack_taunt_card(damage_card,taunt_card)
                    return True
        print("Error no how I got here")
    
    def trade_cards():

        cards_able_to_do_damage=self.get_every_possible_thing_that_can_cause_damage_this_turn()
        opponent_creatures=self.get_opponent_board_creatures()

        if len(cards_able_to_do_damage)==0 :
            return False
        if len(opponent_creatures)==0:
            for damaging_card in cards_able_to_do_damage:
                self.attack_opponent_face(damaging_card)
            return False


        for damage_card in cards_able_to_do_damage:
            for opponent_creature in opponent_creatures:
                score=self.calculate_card_score(damage_card[3],damage_card[4],opponent_creature[2],opponent_creature[3])
                if score<taunt_card[4]:
                    taunt_card[4]=score
        opponent_creatures=sorted(opponent_creatures,key=lambda creature: creature[4])
        if opponent_creatures[0][4]>score_limit:
            for opponent_creature in opponent_creatures:
                for damage_card in cards_able_to_do_damage:
                    score=self.calculate_card_score(damage_card[3],damage_card[4],opponent_creature[2],opponent_creature[3])
                    if score==taunt_card[4]:
                        self.attack_heavy_card(damage_card,taunt_card)
                    return True
        else:
            for damage_card in cards_able_to_do_damage:
                self.attack_opponent_face(damage_card)
            return False
    
    def mulligan_high_cost_cards():
        self.altdriver.wait_for_element('MulliganPopup(Clone)')
        mulligan_cards=self.altdriver.find_elements('MulliganCard_Unit(Clone)')
        replace_panel=self.altdriver.find_element('Replace_Panel')
        button_keep=self.altdriver.find_element('Button_Keep')
        for mulligan_card in mulligan_cards:
            card_cost_object=self.altdriver.find_element('id('+mulligan_card.id+')/Text_Goo')
            card_cost=read_tmp_UGUI_text(self,card_cost_object)
            if(int(card_cost)>3):
                mulligan_card.mobile_dragToElement(replace_panel,2)
        button_keep.mobile_tap()




